using Microsoft.AspNetCore.Identity;

namespace Reservae.Models;

public class User : IdentityUser
{
    public string? Name { get; set; }
    public ICollection<Space> Spaces { get; set; } = new List<Space>();
}
