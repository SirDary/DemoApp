using System;
using System.Collections.Generic;

namespace Demo_Auth.Data.Models;

public partial class Tasks
{
    public int TaskId { get; set; }

    public string TaskTitle { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime DateOfCreate { get; set; }

    public DateTime Deadline { get; set; }

    public int? TaskAuthor { get; set; }

    public int? TasksStatusId { get; set; }

    public virtual Employee? TaskAuthorNavigation { get; set; }

    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

    public virtual TasksStatus? TasksStatus { get; set; }

    public virtual ICollection<Employee> Executors { get; set; } = new List<Employee>();
}
