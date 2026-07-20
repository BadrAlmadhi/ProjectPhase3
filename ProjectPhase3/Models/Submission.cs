using System;
using System.Collections.Generic;

namespace ProjectPhase3.Models;

public partial class Submission
{
    public int Assignmentid { get; set; }

    public int Userid { get; set; }

    public int? Classid { get; set; }

    public DateTime? Submissiontime { get; set; }

    public string? Submissioncontents { get; set; }

    public int? Score { get; set; }

    public virtual Assignment? Assignment { get; set; }

    public virtual Student User { get; set; } = null!;
}
