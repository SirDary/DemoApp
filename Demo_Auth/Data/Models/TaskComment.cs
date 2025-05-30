using System;
using System.Collections.Generic;

namespace Demo_Auth.Data.Models;

public partial class TaskComment
{
    public int CommentId { get; set; }

    public string CommentContent { get; set; } = null!;

    public int? TaskId { get; set; }

    public int? EmployeeId { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Tasks? Tasks { get; set; }
}
