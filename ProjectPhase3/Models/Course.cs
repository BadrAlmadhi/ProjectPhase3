using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Course
{
    public int Catalogid { get; set; }

    public string? Coursename { get; set; }

    public int? Coursenumber { get; set; }

    public string? Subjectabbreviation { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Department? SubjectabbreviationNavigation { get; set; }
}
