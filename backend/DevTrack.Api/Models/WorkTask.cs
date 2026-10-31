
namespace DevTrackAPI.Models;

public class WorkTask
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public WorkTaskStatus Status { get; set; }
        = WorkTaskStatus.ToDo;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }

    // Foreign key
    public int ProjectId { get; set; }

    // Navigation property
    public Project? Project { get; set; }
}
