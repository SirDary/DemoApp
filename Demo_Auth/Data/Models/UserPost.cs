using System;
using System.Collections.Generic;

namespace Demo_Auth.Data.Models;

public partial class UserPost
{
    public int PostId { get; set; }

    public string? PostName { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
