using Microsoft.AspNetCore.Components;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class AdminPaintings : ComponentBase
    {
        [Inject] private IPaintingService PaintingService { get; set; } = default!;
        [Inject] private ILogger<AdminPaintings> Logger { get; set; } = default!;

        private IEnumerable<PaintingDto> paintings = [];
        private bool showEditor = false;
        private bool showDeleteConfirm = false;
        private PaintingDto? editingPainting = null;
        private PaintingDto? paintingToDelete = null;
        private string? errorMessage;
        private string? successMessage;

        protected override async Task OnInitializedAsync()
        {
            await LoadPaintings();
        }

        private async Task LoadPaintings()
        {
            try
            {
                paintings = await PaintingService.GetAllPaintingsAsync();
                errorMessage = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading paintings");
                errorMessage = "Errore nel caricamento dei quadri";
            }
        }

        private async Task HandleSave()
        {
            successMessage = editingPainting == null ? "Quadro creato con successo" : "Quadro modificato con successo";
            HideEditor();
            await LoadPaintings();
        }

        private async Task ExecuteDelete()
        {
            if (paintingToDelete != null)
            {
                try
                {
                    await PaintingService.DeletePaintingAsync(paintingToDelete.Id);
                    successMessage = "Quadro eliminato con successo";
                    await LoadPaintings();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error deleting painting");
                    errorMessage = "Errore durante l'eliminazione del quadro";
                }
            }
            CancelDelete();
        }

        private void ShowEditor(PaintingDto? painting)
        {
            editingPainting = painting;
            showEditor = true;
        }

        private void HideEditor()
        {
            showEditor = false;
            editingPainting = null;
        }

        private void ConfirmDelete(PaintingDto painting)
        {
            paintingToDelete = painting;
            showDeleteConfirm = true;
        }

        private void CancelDelete()
        {
            showDeleteConfirm = false;
            paintingToDelete = null;
        }       
    }
}