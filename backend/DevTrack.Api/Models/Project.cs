
namespace DevTrackAPI.Models;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public ICollection<WorkTask> Tasks { get; set; }
        = new List<WorkTask>();

    public string? OwnerId { get; set; }

    public ApplicationUser? Owner { get; set; }

}
