using System;
using System.Linq;
using System.Net;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Exceptions;
using RenzoAgostini.Server.Mappings;
using RenzoAgostini.Server.Repositories.Interfaces;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Services
{
    public class ShippingOptionService(
        IShippingOptionRepository repository,
        ILogger<ShippingOptionService> logger) : IShippingOptionService
    {
        public async Task<IReadOnlyList<ShippingOptionDto>> GetActiveForCountryAsync(string country)
        {
            var options = await repository.GetActiveAsync();
            var normalizedCountry = (country ?? string.Empty).Trim();
            var isDomestic = IsDomesticDestination(normalizedCountry);

            var filtered = options
                .Where(option => ShouldInclude(option, isDomestic))
                .OrderBy(option => option.IsPickup ? 0 : 1)
                .ThenBy(option => option.Cost)
                .ThenBy(option => option.Name)
                .Select(option => option.ToDto())
                .ToList();

            logger.LogInformation(
                "Resolved {Count} shipping options for country {Country} (domestic: {IsDomestic})",
                filtered.Count,
                normalizedCountry,
                isDomestic);

            return filtered;
        }

        public async Task<IReadOnlyList<ShippingOptionDto>> GetAllAsync()
        {
            var options = await repository.GetAllAsync();
            return options
                .OrderByDescending(option => option.IsActive)
                .ThenBy(option => option.IsPickup ? 0 : 1)
                .ThenBy(option => option.Cost)
                .ThenBy(option => option.Name)
                .Select(option => option.ToDto())
                .ToList();
        }

        public async Task<ShippingOptionDto> CreateAsync(CreateShippingOptionDto dto)
        {
            ValidateDto(dto.Name, dto.Cost, dto.FreeShippingThreshold, dto.SupportsItaly, dto.SupportsInternational, dto.IsPickup);

            if (await repository.ExistsByNameAsync(dto.Name.Trim()))
            {
                throw new ApiException(HttpStatusCode.BadRequest, $"Esiste già un metodo di spedizione chiamato '{dto.Name}'.");
            }

            var entity = MapToEntity(dto);
            entity.CreatedAt = DateTime.UtcNow;

            await repository.AddAsync(entity);
            logger.LogInformation("Created shipping option {Name} ({Id})", entity.Name, entity.Id);

            return entity.ToDto();
        }

        public async Task<ShippingOptionDto> UpdateAsync(int id, UpdateShippingOptionDto dto)
        {
            ValidateDto(dto.Name, dto.Cost, dto.FreeShippingThreshold, dto.SupportsItaly, dto.SupportsInternational, dto.IsPickup);

            var entity = await repository.GetByIdAsync(id)
                ?? throw new ApiException(HttpStatusCode.NotFound, $"Opzione di spedizione ID {id} non trovata.");

            var trimmedName = dto.Name.Trim();
            if (await repository.ExistsByNameAsync(trimmedName, id))
            {
                throw new ApiException(HttpStatusCode.BadRequest, $"Esiste già un metodo di spedizione chiamato '{trimmedName}'.");
            }

            entity.Name = trimmedName;
            entity.Description = dto.Description?.Trim();
            entity.Cost = dto.IsPickup ? 0 : decimal.Round(dto.Cost, 2, MidpointRounding.AwayFromZero);
            entity.FreeShippingThreshold = NormalizeThreshold(dto.FreeShippingThreshold);
            entity.SupportsItaly = dto.SupportsItaly || dto.IsPickup;
            entity.SupportsInternational = dto.SupportsInternational && !dto.IsPickup;
            entity.IsPickup = dto.IsPickup;
            entity.EstimatedDelivery = dto.EstimatedDelivery?.Trim();
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await repository.UpdateAsync(entity);
            logger.LogInformation("Updated shipping option {Name} ({Id})", entity.Name, entity.Id);

            return entity.ToDto();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await repository.GetByIdAsync(id)
                ?? throw new ApiException(HttpStatusCode.NotFound, $"Opzione di spedizione ID {id} non trovata.");

            await repository.DeleteAsync(entity);
            logger.LogInformation("Deleted shipping option {Name} ({Id})", entity.Name, entity.Id);
        }

        private static bool ShouldInclude(ShippingOption option, bool isDomestic)
        {
            if (!option.IsActive)
            {
                return false;
            }

            if (option.IsPickup)
            {
                return isDomestic && option.SupportsItaly;
            }

            return isDomestic
                ? option.SupportsItaly
                : option.SupportsInternational;
        }

        private static ShippingOption MapToEntity(CreateShippingOptionDto dto)
        {
            var entity = new ShippingOption
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Cost = dto.IsPickup ? 0 : decimal.Round(dto.Cost, 2, MidpointRounding.AwayFromZero),
                FreeShippingThreshold = NormalizeThreshold(dto.FreeShippingThreshold),
                SupportsItaly = dto.SupportsItaly || dto.IsPickup,
                SupportsInternational = dto.SupportsInternational && !dto.IsPickup,
                IsPickup = dto.IsPickup,
                EstimatedDelivery = dto.EstimatedDelivery?.Trim(),
                IsActive = dto.IsActive
            };

            return entity;
        }

        private static void ValidateDto(
            string? name,
            decimal cost,
            decimal? threshold,
            bool supportsItaly,
            bool supportsInternational,
            bool isPickup)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Il nome della spedizione è obbligatorio.");
            }

            if (!isPickup && cost < 0)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Il costo della spedizione non può essere negativo.");
            }

            if (!supportsItaly && !supportsInternational && !isPickup)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Seleziona almeno un'area geografica supportata.");
            }

            if (threshold is not null && threshold < 0)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "La soglia per la spedizione gratuita non può essere negativa.");
            }
        }

        private static decimal? NormalizeThreshold(decimal? threshold)
        {
            if (threshold is null)
            {
                return null;
            }

            return threshold <= 0
                ? null
                : decimal.Round(threshold.Value, 2, MidpointRounding.AwayFromZero);
        }

        private static bool IsDomesticDestination(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return true;
            }

            var normalized = country.Trim().ToLowerInvariant();

            return normalized switch
            {
                "italy" or "italia" or "repubblica italiana" or "san marino" or "repubblica di san marino" or
                "vatican" or "vaticano" or "città del vaticano" or "holy see" => true,
                _ => false
            };
        }
    }
}
