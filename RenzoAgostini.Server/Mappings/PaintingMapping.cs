using RenzoAgostini.Server.Entities;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Mappings
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
                painting.Dimensions,
                [.. painting.Images.OrderByDescending(i => i.IsPrimary).Select(img => img.Url)]
            );
        }

        public static Painting ToEntity(this CreatePaintingDto dto)
        {
            return new Painting {
                Id = 0, // ID sarà assegnato dal database
                Slug = dto.Slug,
                Title = dto.Title,
                Description = dto.Description,
                Year = dto.Year,
                Medium = dto.Medium,
                Price =dto.Price,
                IsForSale = dto.IsForSale,
                Dimensions = dto.Dimensions,
                Images = [.. dto.Images.Select(img => new PaintingImage {
                    Url = img.Url,
                    Width = img.Width,
                    Height = img.Height,
                    IsPrimary = img.IsPrimary
                })]
            };
        }

        public static Painting ApplyTo(this CreatePaintingDto dto, Painting target)
        {
            target.Slug = dto.Slug;
            target.Title = dto.Title;
            target.Description = dto.Description;
            target.Year = dto.Year;
            target.Medium = dto.Medium;
            target.Price = dto.Price;
            target.IsForSale = dto.IsForSale;
            target.Dimensions = dto.Dimensions;

            target.Images.Clear();
            target.Images.AddRange(dto.Images.Select(i => new PaintingImage {
                    Url = i.Url,
                    Width = i.Width,
                    Height = i.Height,
                    IsPrimary = i.IsPrimary
                }));
            return target;
        }
    }
}
