using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Assignment
{
    public int Assignmentid { get; set; }

    public string? Categorynames { get; set; }

    public int Classid { get; set; }

    public string? Assignmentname { get; set; }

    public float? Maxpoint { get; set; }

    public string? Content { get; set; }

    public DateTime? Duedate { get; set; }

    public virtual Assignmentcategory? Assignmentcategory { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
