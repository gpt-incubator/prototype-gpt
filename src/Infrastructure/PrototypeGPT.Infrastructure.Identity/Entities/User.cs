using PrototypeGPT.Domain;
using Microsoft.AspNetCore.Identity;

namespace PrototypeGPT.Infrastructure.Identity.Entities;

public class User : IdentityUser<Guid>, IEntity
{
    /// <summary>
    /// Gets or sets the create at
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the update at
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets if is deleted
    /// </summary>
    public bool Deleted { get; set; }
}
