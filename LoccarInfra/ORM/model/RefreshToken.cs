using System;
using System.Collections.Generic;

namespace LoccarInfra.ORM.model;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public virtual User? User { get; set; }
}
