

namespace RemedyLib.Data;

public enum FeedSort { Top, New }
public enum DateRange { Week, Month, AllTime }

public class FeedQuery
{
    public FeedSort Sort { get; set; } = FeedSort.Top;
    public DateRange Range { get; set; } = DateRange.AllTime;
    public string? Category { get; set; }
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 30;
}

public class PostSummary
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public UsageMethod UsageMethod { get; set; }
    public DateTime CreatedUtc { get; set; }
    public int Score { get; set; }

    /// <summary>This device's vote on the post: 1, -1, or 0 for none.</summary>
    public int MyVote { get; set; }
}

public class PostDetail : PostSummary
{
    public string Ingredients { get; set; } = "";
    public string Steps { get; set; } = "";
}

public class NewPost
{
    public string Title { get; set; } = "";
    public string Ingredients { get; set; } = "";
    public string Steps { get; set; } = "";
    public string Category { get; set; } = "";
    public UsageMethod UsageMethod { get; set; }
}

public record VoteResult(int Score, int MyVote);

public record Result<T>(bool Ok, T? Value, string? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}
