namespace RenzoAgostini.Server.Emailing.Models
{
    public sealed record EmailMessage(EmailAddress? From, EmailAddress? ReplyTo, string? TextBody, string? HtmlBody)
    {
        public List<EmailAddress> To { get; init; } = [];
        public List<EmailAddress> Cc { get; init; } = [];
        public List<EmailAddress> Bcc { get; init; } = [];
        public string Subject { get; init; } = "";
        public List<EmailAttachment> Attachments { get; init; } = [];
    }
}
