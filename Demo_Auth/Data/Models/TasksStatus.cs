using System;
using System.Collections.Generic;

namespace Demo_Auth.Data.Models;

public partial class TasksStatus
{
    public int TasksStatusId { get; set; }

    public string? TasksStatusName { get; set; }

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();
}
