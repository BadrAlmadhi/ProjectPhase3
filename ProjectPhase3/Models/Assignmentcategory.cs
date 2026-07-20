using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Assignmentcategory
{
    public string Categorynames { get; set; } = null!;

    public float? Gradingweight { get; set; }

    public int Classid { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Class Class { get; set; } = null!;
}
