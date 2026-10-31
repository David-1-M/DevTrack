
using Microsoft.AspNetCore.Identity;

namespace DevTrackAPI.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public ICollection<Project> Projects { get; set; }
        = new List<Project>();
}
