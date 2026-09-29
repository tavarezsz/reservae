using Microsoft.AspNetCore.Identity;

namespace Reservae.Models;

public class User : IdentityUser
{
    public string? Name { get; set; }
    public ICollection<Space> Spaces { get; set; } = new List<Space>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
