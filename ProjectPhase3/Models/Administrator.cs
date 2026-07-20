using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Administrator
{
    public int Userid { get; set; }

    public virtual User User { get; set; } = null!;
}
