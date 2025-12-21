namespace RenzoAgostini.Shared.DTOs
{
    public record CreateCustomOrderDto(
        string CustomerEmail,
        string Description,
        string? AttachmentPath
    );
}
