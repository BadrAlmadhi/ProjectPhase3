using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Class
{
    public int Classid { get; set; }

    public int? Catalogid { get; set; }

    public int? Professorid { get; set; }

    public string? Semester { get; set; }

    public string? Location { get; set; }

    public TimeOnly? Starttime { get; set; }

    public TimeOnly? Endtime { get; set; }

    public virtual ICollection<Assignmentcategory> Assignmentcategories { get; set; } = new List<Assignmentcategory>();

    public virtual Course? Catalog { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Professor? Professor { get; set; }
}
