using RenzoAgostini.Server.Entities;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Mappings
{
    public static class CustomOrderMapping
    {
        public static CustomOrderDto ToDto(this CustomOrder customOrder)
        {
            return new CustomOrderDto(
                customOrder.Id,
                customOrder.CustomerEmail,
                customOrder.Description,
                customOrder.AttachmentOriginalName,
                customOrder.Status,
                customOrder.CreatedAt,
                customOrder.AcceptedAt,
                customOrder.QuotedPrice,
                customOrder.ArtistNotes,
                customOrder.PaintingId,
                customOrder.AccessCode
            );
        }
    }
}
