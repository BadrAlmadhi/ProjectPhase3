using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Professor
{
    public int Userid { get; set; }

    public string? Subjectabbreviation { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Department? SubjectabbreviationNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
