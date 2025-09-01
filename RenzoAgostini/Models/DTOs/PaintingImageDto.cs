namespace RenzoAgostini.Models.DTOs
{
    public record PaintingImageDto(
       string Url,
       int? Width,
       int? Height,
       bool IsPrimary = false
   );
}
