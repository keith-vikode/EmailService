using System;

namespace EmailService.Core
{
    /// <summary>
    /// Enumerates the different levels of content logging for emails.
    /// </summary>
    [Flags]
    public enum EmailContentLogLevel
    {
        None = 0,
        Subject = 1,
        Body = 2,
        Recipients = 4,
        All = Subject | Body | Recipients,
        SubjectAndRecipients = Subject | Recipients
    }
}
