using System;
using System.Collections.Generic;

namespace LoccarInfra.ORM.model;

public partial class AuditLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? IpAddress { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
