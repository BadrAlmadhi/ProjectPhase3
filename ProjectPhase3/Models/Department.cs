using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Department
{
    public string Subjectabbreviation { get; set; } = null!;

    public string? Departmentname { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Professor> Professors { get; set; } = new List<Professor>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
