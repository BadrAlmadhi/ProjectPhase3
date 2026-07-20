using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Enrollment
{
    public int Userid { get; set; }

    public int Classid { get; set; }

    public string? Grade { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Student User { get; set; } = null!;
}
