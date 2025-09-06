namespace RenzoAgostini.Shared.DTOs
{
    public record AddressDto(
            string Street,
            string City,
            string PostalCode,
            string Country
        );
}