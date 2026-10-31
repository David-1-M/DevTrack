
using System.ComponentModel.DataAnnotations;
using DevTrackAPI.Models;

namespace DevTrackAPI.DTOs;

public class UpdateWorkTaskDto
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }

    public WorkTaskStatus Status { get; set; }
        = WorkTaskStatus.ToDo;

    public DateTime? DueDate { get; set; }
}
