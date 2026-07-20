using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Student
{
    public int Userid { get; set; }

    public string? Subjectabbreviation { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Department? SubjectabbreviationNavigation { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual User User { get; set; } = null!;
}
