using RenzoAgostini.Entities;
using RenzoAgostini.Models.DTOs;

namespace RenzoAgostini.Models
{
    public static class PaintingMapping
    {
        public static PaintingDto ToDto(this Painting painting)
        {
            return new PaintingDto(
                painting.Id,
                painting.Slug,
                painting.Title,
                painting.Description,
                painting.Year,
                painting.Medium,
                painting.Price,
                painting.IsForSale,
                [.. painting.Images.Select(img => img.Url)]
            );
        }

        public static Painting ToEntity(this CreatePaintingDto dto)
        {
            return new Painting(
                0, // ID sarà assegnato dal database
                dto.Slug,
                dto.Title,
                dto.Description,
                dto.Year,
                dto.Medium,
                dto.Price,
                dto.IsForSale,
                [.. dto.Images.Select(img => new PaintingImage(
                    img.Url,
                    img.Width,
                    img.Height,
                    img.IsPrimary
                ))]
            );
        }
    }
}
