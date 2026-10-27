namespace RemedyLib.Data;

public enum UsageMethod { Skin, Eat, Breathe }

public class Post
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Ingredients { get; set; } = "";
    public string Steps { get; set; } = "";
    public string Category { get; set; } = "";
    public UsageMethod UsageMethod { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string AuthorDeviceId { get; set; } = "";

    /// <summary>Set automatically when anyone reports the post. Moderators review flagged posts.</summary>
    public bool IsFlagged { get; set; }

    /// <summary>Hidden posts never appear in the feed. Set by a moderator or by the auto-hide threshold.</summary>
    public bool IsHidden { get; set; }

    public List<Vote> Votes { get; set; } = new();
    public List<Report> Reports { get; set; } = new();
}

public class Vote
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public string DeviceId { get; set; } = "";

    /// <summary>+1 for upvote, -1 for downvote.</summary>
    public int Value { get; set; }
    public DateTime CreatedUtc { get; set; }
}

public class Report
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public string DeviceId { get; set; } = "";
    public string? Reason { get; set; }
    public DateTime CreatedUtc { get; set; }
}
