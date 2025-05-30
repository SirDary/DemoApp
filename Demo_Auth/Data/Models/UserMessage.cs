using System;
using System.Collections.Generic;

namespace Demo_Auth.Data.Models;

public partial class UserMessage
{
    public int MessageId { get; set; }

    public string MessageContext { get; set; } = null!;

    public DateTime DateOfCreate { get; set; }

    public string? Image { get; set; }

    public string? Video { get; set; }

    public int? ReplyToMessage { get; set; }

    public int? EmployeeId { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<UserMessage> InverseReplyToMessageNavigation { get; set; } = new List<UserMessage>();

    public virtual UserMessage? ReplyToMessageNavigation { get; set; }
}
