using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services
{
    public class BiographyService(IBiographyRepository repository) : IBiographyService
    {
        public async Task<BiographyDto> GetAsync()
        {
            var bio = await repository.GetSingletonAsync();
            return new BiographyDto(bio?.Content ?? "", bio?.ImageUrl);
        }

        public async Task UpdateAsync(BiographyDto dto)
        {
            var bio = await repository.GetSingletonAsync();
            if (bio == null)
            {
                bio = new Biography { Content = dto.Content, ImageUrl = dto.ImageUrl };
                await repository.AddAsync(bio);
            }
            else
            {
                bio.Content = dto.Content;
                bio.ImageUrl = dto.ImageUrl;
                await repository.UpdateAsync(bio);
            }
        }
    }
}
