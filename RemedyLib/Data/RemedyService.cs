using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace RemedyLib.Data;

/// <summary>Supplies a random, stable ID for this install. Implemented in the MAUI project.</summary>
public interface IDeviceIdProvider
{
    string GetDeviceId();
}

/// <summary>
/// All database access goes through here. Razor components never touch AppDbContext.
/// Register as a singleton; it creates a short-lived DbContext for every call.
/// Any method can throw if the phone has no internet, so wrap UI calls in try/catch.
/// </summary>
public class RemedyService
{
    private const int MaxPostsPerHour = 5;
    private const int AutoHideAtReports = 3; // raise this if it hides posts too eagerly

    // Placeholder list. Add your own words.
    private static readonly string[] BlockedWords = { "badword1", "badword2" };

    private static readonly Regex HtmlTags = new("<[^>]*>", RegexOptions.Compiled);

    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IDeviceIdProvider _device;

    public RemedyService(IDbContextFactory<AppDbContext> factory, IDeviceIdProvider device)
    {
        _factory = factory;
        _device = device;
    }

    // ---------- Reading ----------

    /// <summary>
    /// Returns feed posts with Score and MyVote filled in.
    /// Ingredients and Steps are left empty here; call GetPostAsync for the full post.
    /// </summary>
    public async Task<List<Post>> GetFeedAsync(
        FeedSort sort = FeedSort.Top,
        DateRange range = DateRange.AllTime,
        Category? category = null,
        int page = 0,
        int pageSize = 30)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var deviceId = _device.GetDeviceId();

        var query = db.Posts.AsNoTracking().Where(p => !p.IsHidden);

        if (range != DateRange.AllTime)
        {
            var cutoff = DateTime.UtcNow.AddDays(range == DateRange.Week ? -7 : -30);
            query = query.Where(p => p.CreatedUtc >= cutoff);
        }

        if (category.HasValue)
        {
            var wanted = category.Value;
            query = query.Where(p => p.Category == wanted);
        }

        var ordered = sort == FeedSort.Top
            ? query.OrderByDescending(p => p.Votes.Sum(v => v.Value)).ThenByDescending(p => p.CreatedUtc)
            : query.OrderByDescending(p => p.CreatedUtc);

        var size = Math.Clamp(pageSize, 1, 100);
        var skip = Math.Max(page, 0) * size;

        return await ordered
            .Skip(skip)
            .Take(size)
            .Select(p => new Post
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                UsageMethod = p.UsageMethod,
                CreatedUtc = p.CreatedUtc,
                Score = p.Votes.Sum(v => v.Value),
                MyVote = p.Votes.Where(v => v.DeviceId == deviceId).Select(v => v.Value).FirstOrDefault()
            })
            .ToListAsync();
    }

    /// <summary>Returns one full post (with Ingredients and Steps), or null if it doesn't exist.</summary>
    public async Task<Post?> GetPostAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var deviceId = _device.GetDeviceId();

        return await db.Posts.AsNoTracking()
            .Where(p => p.Id == id && !p.IsHidden)
            .Select(p => new Post
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Ingredients = p.Ingredients,
                Steps = p.Steps,
                Category = p.Category,
                UsageMethod = p.UsageMethod,
                CreatedUtc = p.CreatedUtc,
                Score = p.Votes.Sum(v => v.Value),
                MyVote = p.Votes.Where(v => v.DeviceId == deviceId).Select(v => v.Value).FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    // ---------- Creating ----------

    /// <summary>
    /// Saves a new post. Only Title, Description, Ingredients, Steps, Category and UsageMethod
    /// are read from the post you pass in.
    /// Returns null on success, or an error message to show the user.
    /// </summary>
    public async Task<string?> CreatePostAsync(Post input)
    {
        var title = Clean(input.Title);
        var description = Clean(input.Description);
        var ingredients = Clean(input.Ingredients);
        var steps = Clean(input.Steps);

        if (title.Length < 3 || title.Length > 100)
            return "Title must be 3 to 100 characters.";
        if (description.Length > 300)
            return "Description must be 300 characters or fewer.";
        if (ingredients.Length < 1 || ingredients.Length > 2000)
            return "Ingredients are required (up to 2000 characters).";
        if (steps.Length < 1 || steps.Length > 4000)
            return "Steps are required (up to 4000 characters).";
        if (!Enum.IsDefined(input.Category))
            return "Please choose a category.";
        if (!Enum.IsDefined(input.UsageMethod))
            return "Please choose how the remedy is used.";
        if (ContainsBlockedWord($"{title} {description} {ingredients} {steps}"))
            return "Your post contains words that aren't allowed.";

        var deviceId = _device.GetDeviceId();
        await using var db = await _factory.CreateDbContextAsync();

        var since = DateTime.UtcNow.AddHours(-1);
        var recent = await db.Posts.CountAsync(p => p.AuthorDeviceId == deviceId && p.CreatedUtc > since);
        if (recent >= MaxPostsPerHour)
            return "You're posting too fast. Please try again later.";

        db.Posts.Add(new Post
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Ingredients = ingredients,
            Steps = steps,
            Category = input.Category,
            UsageMethod = input.UsageMethod,
            CreatedUtc = DateTime.UtcNow,
            AuthorDeviceId = deviceId
        });
        await db.SaveChangesAsync();
        return null;
    }

    // ---------- Voting ----------

    /// <summary>
    /// direction: 1 = upvote, -1 = downvote.
    /// Tapping the same direction again removes the vote. Tapping the other direction switches it.
    /// Returns the post with fresh Score and MyVote, or null if the vote couldn't be saved.
    /// </summary>
    public async Task<Post?> VoteAsync(Guid postId, int direction)
    {
        if (direction != 1 && direction != -1) return null;

        var deviceId = _device.GetDeviceId();

        await using (var db = await _factory.CreateDbContextAsync())
        {
            var exists = await db.Posts.AnyAsync(p => p.Id == postId && !p.IsHidden);
            if (!exists) return null;

            var vote = await db.Votes.FirstOrDefaultAsync(v => v.PostId == postId && v.DeviceId == deviceId);
            if (vote is null)
            {
                db.Votes.Add(new Vote
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    DeviceId = deviceId,
                    Value = direction,
                    CreatedUtc = DateTime.UtcNow
                });
            }
            else if (vote.Value == direction)
            {
                db.Votes.Remove(vote);
            }
            else
            {
                vote.Value = direction;
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return null; // a double-tap race hit the unique index; the user can tap again
            }
        }

        // Re-read from the server so the screen always shows the authoritative count.
        return await GetPostAsync(postId);
    }

    // ---------- Moderation hooks ----------

    /// <summary>Reports a post. Returns false only if the post no longer exists.</summary>
    public async Task<bool> ReportPostAsync(Guid postId, string? reason = null)
    {
        var deviceId = _device.GetDeviceId();
        await using var db = await _factory.CreateDbContextAsync();

        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null) return false;

        var already = await db.Reports.AnyAsync(r => r.PostId == postId && r.DeviceId == deviceId);
        if (already) return true; // reporting twice is harmless

        var cleaned = string.IsNullOrWhiteSpace(reason) ? null : Clean(reason);
        if (cleaned is { Length: > 200 }) cleaned = cleaned[..200];

        db.Reports.Add(new Report
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            DeviceId = deviceId,
            Reason = cleaned,
            CreatedUtc = DateTime.UtcNow
        });
        post.IsFlagged = true;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return true; // duplicate report from a race; already recorded
        }

        var count = await db.Reports.CountAsync(r => r.PostId == postId);
        if (count >= AutoHideAtReports && !post.IsHidden)
        {
            post.IsHidden = true;
            await db.SaveChangesAsync();
        }
        return true;
    }

    // ---------- Helpers ----------

    private static string Clean(string? text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        var s = text.Replace("\0", "").Replace("\r\n", "\n");
        s = HtmlTags.Replace(s, "");
        return s.Trim();
    }

    private static bool ContainsBlockedWord(string text) =>
        BlockedWords.Any(w => text.Contains(w, StringComparison.OrdinalIgnoreCase));
}
