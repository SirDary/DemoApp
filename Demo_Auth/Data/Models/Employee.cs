using System;
using System.Collections.Generic;

namespace Demo_Auth.Data.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? FullName { get; set; }

    public string? NumberPhone { get; set; }

    public int? PostId { get; set; }

    public virtual UserPost? Post { get; set; }

    public string? AppUserId { get; set; }
    public virtual AppUser? User { get; set; }

    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

    public virtual ICollection<Tasks> Tasks { get; set; } = new List<Tasks>();

    public virtual ICollection<UserMessage> UserMessages { get; set; } = new List<UserMessage>();

    public virtual ICollection<Tasks> TasksNavigation { get; set; } = new List<Tasks>();
}
