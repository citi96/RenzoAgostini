using Microsoft.AspNetCore.Components;
using RenzoAgostini.Client.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class AdminShipping : ComponentBase
    {
        [Inject] private IShippingClient ShippingClient { get; set; } = default!;
        [Inject] private ILogger<AdminShipping> Logger { get; set; } = default!;

        private List<ShippingOptionDto> shippingOptions = [];
        private bool isLoading = true;
        private bool isSaving;
        private string? errorMessage;
        private string? successMessage;
        private bool showEditor;
        private bool isEditing;
        private int editingId;
        private string? formError;
        private ShippingOptionFormModel form = new();
        private bool showDeleteConfirm;
        private ShippingOptionDto? optionToDelete;

        protected override async Task OnInitializedAsync()
        {
            await LoadOptionsAsync();
        }

        private async Task LoadOptionsAsync()
        {
            try
            {
                isLoading = true;
                shippingOptions = (await ShippingClient.GetAllAsync())
                    .OrderByDescending(o => o.IsActive)
                    .ThenBy(o => o.IsPickup ? 0 : 1)
                    .ThenBy(o => o.Cost)
                    .ThenBy(o => o.Name)
                    .ToList();

                errorMessage = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading shipping options");
                errorMessage = "Errore nel caricamento delle modalità di spedizione.";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        private void ShowCreateModal()
        {
            form = new ShippingOptionFormModel
            {
                Name = string.Empty,
                Description = string.Empty,
                Cost = 0,
                FreeShippingThreshold = null,
                SupportsItaly = true,
                SupportsInternational = false,
                IsPickup = false,
                EstimatedDelivery = string.Empty,
                IsActive = true
            };
            formError = null;
            isEditing = false;
            showEditor = true;
        }

        private void ShowEditModal(ShippingOptionDto option)
        {
            form = new ShippingOptionFormModel
            {
                Name = option.Name,
                Description = option.Description,
                Cost = option.Cost,
                FreeShippingThreshold = option.FreeShippingThreshold,
                SupportsItaly = option.SupportsItaly,
                SupportsInternational = option.SupportsInternational,
                IsPickup = option.IsPickup,
                EstimatedDelivery = option.EstimatedDelivery,
                IsActive = option.IsActive
            };
            formError = null;
            isEditing = true;
            editingId = option.Id;
            showEditor = true;
        }

        private void CloseEditor()
        {
            showEditor = false;
            formError = null;
        }

        private async Task SaveAsync()
        {
            if (isSaving)
            {
                return;
            }

            if (!ValidateForm(out var validationError))
            {
                formError = validationError;
                return;
            }

            formError = null;
            isSaving = true;

            try
            {
                var normalized = NormalizeForm(form);

                if (isEditing)
                {
                    var updateDto = new UpdateShippingOptionDto(
                        normalized.Name,
                        normalized.Description,
                        normalized.Cost,
                        normalized.FreeShippingThreshold,
                        normalized.SupportsItaly,
                        normalized.SupportsInternational,
                        normalized.IsPickup,
                        normalized.EstimatedDelivery,
                        normalized.IsActive);

                    var updated = await ShippingClient.UpdateAsync(editingId, updateDto);
                    successMessage = $"Modalità '{updated.Name}' aggiornata correttamente.";
                    Logger.LogInformation("Updated shipping option {Id}", editingId);
                }
                else
                {
                    var createDto = new CreateShippingOptionDto(
                        normalized.Name,
                        normalized.Description,
                        normalized.Cost,
                        normalized.FreeShippingThreshold,
                        normalized.SupportsItaly,
                        normalized.SupportsInternational,
                        normalized.IsPickup,
                        normalized.EstimatedDelivery,
                        normalized.IsActive);

                    var created = await ShippingClient.CreateAsync(createDto);
                    successMessage = $"Modalità '{created.Name}' creata con successo.";
                    Logger.LogInformation("Created shipping option {Id}", created.Id);
                }

                errorMessage = null;
                showEditor = false;
                await LoadOptionsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving shipping option");
                errorMessage = ex.Message;
            }
            finally
            {
                isSaving = false;
                StateHasChanged();
            }
        }

        private bool ValidateForm(out string? validationError)
        {
            if (string.IsNullOrWhiteSpace(form.Name))
            {
                validationError = "Il nome è obbligatorio.";
                return false;
            }

            if (!form.IsPickup && form.Cost < 0)
            {
                validationError = "Il costo non può essere negativo.";
                return false;
            }

            if (!form.IsPickup && !form.SupportsItaly && !form.SupportsInternational)
            {
                validationError = "Seleziona almeno un'area geografica supportata.";
                return false;
            }

            if (form.FreeShippingThreshold is decimal threshold && threshold < 0)
            {
                validationError = "La soglia per la spedizione gratuita non può essere negativa.";
                return false;
            }

            validationError = null;
            return true;
        }

        private static ShippingOptionFormModel NormalizeForm(ShippingOptionFormModel source)
        {
            var trimmed = source with
            {
                Name = source.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(source.Description) ? null : source.Description.Trim(),
                EstimatedDelivery = string.IsNullOrWhiteSpace(source.EstimatedDelivery) ? null : source.EstimatedDelivery.Trim(),
                FreeShippingThreshold = source.FreeShippingThreshold is { } threshold && threshold <= 0 ? null : source.FreeShippingThreshold
            };

            var sanitized = trimmed.IsPickup
                ? trimmed with
                {
                    Cost = 0,
                    FreeShippingThreshold = null,
                    SupportsItaly = true,
                    SupportsInternational = false
                }
                : trimmed;

            sanitized = sanitized with
            {
                Cost = Math.Round(sanitized.Cost, 2, MidpointRounding.AwayFromZero)
            };

            if (sanitized.FreeShippingThreshold is decimal value)
            {
                sanitized = sanitized with
                {
                    FreeShippingThreshold = Math.Round(value, 2, MidpointRounding.AwayFromZero)
                };
            }

            return sanitized;
        }

        private void OnPickupChanged(ChangeEventArgs _)
        {
            if (form.IsPickup)
            {
                form = form with
                {
                    Cost = 0,
                    FreeShippingThreshold = null,
                    SupportsItaly = true,
                    SupportsInternational = false
                };
            }

            StateHasChanged();
        }

        private void ConfirmDelete(ShippingOptionDto option)
        {
            optionToDelete = option;
            showDeleteConfirm = true;
        }

        private void CancelDelete()
        {
            showDeleteConfirm = false;
            optionToDelete = null;
        }

        private async Task DeleteAsync()
        {
            if (optionToDelete is null || isSaving)
            {
                return;
            }

            isSaving = true;

            try
            {
                await ShippingClient.DeleteAsync(optionToDelete.Id);
                successMessage = $"Modalità '{optionToDelete.Name}' eliminata correttamente.";
                errorMessage = null;
                showDeleteConfirm = false;
                optionToDelete = null;
                await LoadOptionsAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting shipping option {Id}", optionToDelete?.Id);
                errorMessage = "Errore nell'eliminazione della modalità di spedizione.";
            }
            finally
            {
                isSaving = false;
                StateHasChanged();
            }
        }

        private record ShippingOptionFormModel
        {
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; } = string.Empty;
            public decimal Cost { get; set; }
            public decimal? FreeShippingThreshold { get; set; }
            public bool SupportsItaly { get; set; } = true;
            public bool SupportsInternational { get; set; }
            public bool IsPickup { get; set; }
            public string? EstimatedDelivery { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
        }
    }
}
