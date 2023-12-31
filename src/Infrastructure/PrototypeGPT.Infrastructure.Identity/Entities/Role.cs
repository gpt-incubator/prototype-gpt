﻿using PrototypeGPT.Domain;
using Microsoft.AspNetCore.Identity;

namespace PrototypeGPT.Infrastructure.Identity.Entities;

public class Role : IdentityRole<Guid>, IEntity
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
    /// 
    /// </summary>
    public bool Deleted { get; set; }
}
