using Microsoft.AspNetCore.Http;

namespace RenzoAgostini.Shared.DTOs
{
    public record CreateCustomOrderDto(
        string CustomerEmail,
        string Description,
        IFormFile? Attachment
    );
}
