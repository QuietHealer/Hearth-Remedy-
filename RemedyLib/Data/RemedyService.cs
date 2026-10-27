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
    public static readonly IReadOnlyList<string> Categories = new[]
    {
        "Cold & Flu", "Cough", "Sore Throat", "Upset Stomach",
        "Headache", "Skin", "Sleep", "Other"
    };

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

    public async Task<List<PostSummary>> GetFeedAsync(FeedQuery q)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var deviceId = _device.GetDeviceId();

        var query = db.Posts.AsNoTracking().Where(p => !p.IsHidden);

        if (q.Range != DateRange.AllTime)
        {
            var cutoff = DateTime.UtcNow.AddDays(q.Range == DateRange.Week ? -7 : -30);
            query = query.Where(p => p.CreatedUtc >= cutoff);
        }

        var category = q.Category;
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        var ordered = q.Sort == FeedSort.Top
            ? query.OrderByDescending(p => p.Votes.Sum(v => v.Value)).ThenByDescending(p => p.CreatedUtc)
            : query.OrderByDescending(p => p.CreatedUtc);

        var size = Math.Clamp(q.PageSize, 1, 100);
        var page = Math.Max(q.Page, 0);

        return await ordered
            .Skip(page * size)
            .Take(size)
            .Select(p => new PostSummary
            {
                Id = p.Id,
                Title = p.Title,
                Category = p.Category,
                UsageMethod = p.UsageMethod,
                CreatedUtc = p.CreatedUtc,
                Score = p.Votes.Sum(v => v.Value),
                MyVote = p.Votes.Where(v => v.DeviceId == deviceId).Select(v => v.Value).FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<PostDetail?> GetPostAsync(Guid id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var deviceId = _device.GetDeviceId();

        return await db.Posts.AsNoTracking()
            .Where(p => p.Id == id && !p.IsHidden)
            .Select(p => new PostDetail
            {
                Id = p.Id,
                Title = p.Title,
                Category = p.Category,
                UsageMethod = p.UsageMethod,
                CreatedUtc = p.CreatedUtc,
                Ingredients = p.Ingredients,
                Steps = p.Steps,
                Score = p.Votes.Sum(v => v.Value),
                MyVote = p.Votes.Where(v => v.DeviceId == deviceId).Select(v => v.Value).FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    // ---------- Creating ----------

    public async Task<Result<Guid>> CreatePostAsync(NewPost input)
    {
        var title = Clean(input.Title);
        var ingredients = Clean(input.Ingredients);
        var steps = Clean(input.Steps);
        var category = input.Category?.Trim() ?? "";

        if (title.Length < 3 || title.Length > 100)
            return Result<Guid>.Fail("Title must be 3 to 100 characters.");
        if (ingredients.Length < 1 || ingredients.Length > 2000)
            return Result<Guid>.Fail("Ingredients are required (up to 2000 characters).");
        if (steps.Length < 1 || steps.Length > 4000)
            return Result<Guid>.Fail("Steps are required (up to 4000 characters).");
        if (!Categories.Contains(category))
            return Result<Guid>.Fail("Please choose a category.");
        if (!Enum.IsDefined(input.UsageMethod))
            return Result<Guid>.Fail("Please choose how the remedy is used.");
        if (ContainsBlockedWord($"{title} {ingredients} {steps}"))
            return Result<Guid>.Fail("Your post contains words that aren't allowed.");

        var deviceId = _device.GetDeviceId();
        await using var db = await _factory.CreateDbContextAsync();

        var since = DateTime.UtcNow.AddHours(-1);
        var recent = await db.Posts.CountAsync(p => p.AuthorDeviceId == deviceId && p.CreatedUtc > since);
        if (recent >= MaxPostsPerHour)
            return Result<Guid>.Fail("You're posting too fast. Please try again later.");

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = title,
            Ingredients = ingredients,
            Steps = steps,
            Category = category,
            UsageMethod = input.UsageMethod,
            CreatedUtc = DateTime.UtcNow,
            AuthorDeviceId = deviceId
        };
        db.Posts.Add(post);
        await db.SaveChangesAsync();
        return Result<Guid>.Success(post.Id);
    }

    // ---------- Voting ----------

    /// <summary>
    /// direction: 1 = upvote, -1 = downvote.
    /// Tapping the same direction again removes the vote. Tapping the other direction switches it.
    /// </summary>
    public async Task<Result<VoteResult>> VoteAsync(Guid postId, int direction)
    {
        if (direction != 1 && direction != -1)
            return Result<VoteResult>.Fail("Invalid vote.");

        var deviceId = _device.GetDeviceId();
        await using var db = await _factory.CreateDbContextAsync();

        var exists = await db.Posts.AnyAsync(p => p.Id == postId && !p.IsHidden);
        if (!exists)
            return Result<VoteResult>.Fail("That post no longer exists.");

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
            // Double-tap race hit the unique index. Safe to ask the user to retry.
            return Result<VoteResult>.Fail("Couldn't save your vote. Please try again.");
        }

        // Re-read from the server so the screen always shows the authoritative count.
        var score = await db.Votes.Where(v => v.PostId == postId).SumAsync(v => v.Value);
        var mine = await db.Votes.Where(v => v.PostId == postId && v.DeviceId == deviceId)
            .Select(v => v.Value).FirstOrDefaultAsync();
        return Result<VoteResult>.Success(new VoteResult(score, mine));
    }

    // ---------- Moderation hooks ----------

    public async Task<Result<bool>> ReportPostAsync(Guid postId, string? reason = null)
    {
        var deviceId = _device.GetDeviceId();
        await using var db = await _factory.CreateDbContextAsync();

        var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post is null)
            return Result<bool>.Fail("That post no longer exists.");

        var already = await db.Reports.AnyAsync(r => r.PostId == postId && r.DeviceId == deviceId);
        if (already)
            return Result<bool>.Success(true); // reporting twice is harmless

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
            return Result<bool>.Success(true); // duplicate report from a race; already recorded
        }

        var count = await db.Reports.CountAsync(r => r.PostId == postId);
        if (count >= AutoHideAtReports && !post.IsHidden)
        {
            post.IsHidden = true;
            await db.SaveChangesAsync();
        }
        return Result<bool>.Success(true);
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
