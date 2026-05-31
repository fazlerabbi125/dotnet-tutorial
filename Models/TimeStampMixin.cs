namespace TutorialProj.Models;

public abstract class TimeStampMixin
{
    public DateTime CreatedAt { get; protected set; } // set by EF Core on insert
    public DateTime UpdatedAt { get; protected set; } // set by EF Core on insert and update
}