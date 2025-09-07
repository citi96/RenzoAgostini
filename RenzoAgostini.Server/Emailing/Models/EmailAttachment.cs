namespace RenzoAgostini.Server.Emailing.Models
{
    public sealed class EmailAttachment
    {
        public string FileName { get; init; } = default!;
        public string ContentType { get; init; } = "application/octet-stream";
        public byte[] Content { get; init; } = default!;
    }
}
