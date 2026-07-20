using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class User
{
    public int Userid { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public DateOnly? Dob { get; set; }

    public virtual Administrator? Administrator { get; set; }

    public virtual Professor? Professor { get; set; }

    public virtual Student? Student { get; set; }
}
