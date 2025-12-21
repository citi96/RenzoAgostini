using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Shared.Contracts
{
    public interface IBiographyService
    {
        Task<BiographyDto> GetAsync();
        Task UpdateAsync(BiographyDto dto);
    }
}
