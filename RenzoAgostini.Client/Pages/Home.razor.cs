using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Client.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private IPaintingService PaintingService { get; set; } = default!;
        [Inject] private ILogger<Home> Logger { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

        protected IEnumerable<PaintingDto>? allPaintings;
        protected IEnumerable<PaintingDto>? filteredPaintings;
        protected string? errorMessage;
        protected bool isLoading = true;
        protected string selectedFilter = "all";
        protected int displayCount = 6; // Numero iniziale di quadri da mostrare
        protected const int LoadMoreIncrement = 6; // Quanti quadri caricare ogni volta

        // Newsletter
        protected string newsletterEmail = string.Empty;
        protected bool isSubscribing = false;
        protected string subscriptionMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadPaintings();

            // Inizializza animazioni dopo il render
            await Task.Delay(100);
            await InitializeAnimations();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Smooth scroll behavior per i link interni
                await JSRuntime.InvokeVoidAsync("initializeSmoothScroll");

                // Animazione contatori nella sezione stats
                await JSRuntime.InvokeVoidAsync("animateCounters");

                // Intersection Observer per animazioni on-scroll
                await JSRuntime.InvokeVoidAsync("initializeScrollAnimations");
            }
        }

        private async Task LoadPaintings()
        {
            try
            {
                isLoading = true;
                errorMessage = null;
                StateHasChanged();

                // Simula un piccolo delay per mostrare l'animazione di caricamento
                await Task.Delay(300);

                allPaintings = await PaintingService.GetAllPaintingsAsync();
                FilterPaintings(selectedFilter);

                Logger.LogInformation("Loaded {Count} paintings successfully", allPaintings?.Count() ?? 0);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading paintings on home page");
                errorMessage = "Errore nel caricamento dei quadri. Riprova più tardi.";
                await ShowErrorToast("Errore nel caricamento delle opere d'arte");
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        protected async Task ReloadPaintings()
        {
            displayCount = 6; // Reset display count
            await LoadPaintings();
            await ShowSuccessToast("Opere ricaricate con successo!");
        }

        protected void FilterPaintings(string filter)
        {
            if (allPaintings == null) return;

            selectedFilter = filter;
            displayCount = 6; // Reset display count when changing filter

            filteredPaintings = filter switch
            {
                "sale" => allPaintings.Where(p => p.IsForSale),
                "new" => allPaintings.Where(p => p.Year >= DateTime.Now.Year - 1),
                "featured" => allPaintings.Where(p => p.IsForSale && p.Price >= 1000), // Esempio logica featured
                _ => allPaintings
            };

            // Ordina per anno decrescente e poi per titolo
            filteredPaintings = filteredPaintings.OrderByDescending(p => p.Year).ThenBy(p => p.Title);

            StateHasChanged();

            Logger.LogInformation("Filtered paintings: {Filter} -> {Count} results", filter, filteredPaintings.Count());
        }

        protected void LoadMore()
        {
            displayCount += LoadMoreIncrement;
            StateHasChanged();

            Logger.LogInformation("Display count increased to {DisplayCount}", displayCount);
        }

        protected async Task SubscribeNewsletter()
        {
            if (string.IsNullOrWhiteSpace(newsletterEmail) || isSubscribing)
                return;

            // Validazione email base
            if (!IsValidEmail(newsletterEmail))
            {
                await ShowErrorToast("Inserisci un indirizzo email valido");
                return;
            }

            try
            {
                isSubscribing = true;
                subscriptionMessage = string.Empty;
                StateHasChanged();

                // Simula chiamata API (sostituire con chiamata reale)
                await Task.Delay(1000);

                // TODO: Implementare la logica di iscrizione newsletter
                // await NewsletterService.SubscribeAsync(newsletterEmail);

                subscriptionMessage = "✅ Iscrizione completata! Controlla la tua email.";
                newsletterEmail = string.Empty;

                await ShowSuccessToast("Iscritto alla newsletter con successo!");

                Logger.LogInformation("Newsletter subscription for email: {Email}", newsletterEmail);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error subscribing to newsletter: {Email}", newsletterEmail);
                subscriptionMessage = "❌ Errore durante l'iscrizione. Riprova.";
                await ShowErrorToast("Errore durante l'iscrizione alla newsletter");
            }
            finally
            {
                isSubscribing = false;
                StateHasChanged();
            }
        }

        // Utility methods
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task InitializeAnimations()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("initializeHomeAnimations");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing home page animations");
            }
        }

        private async Task ShowSuccessToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "success");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing success toast");
            }
        }

        private async Task ShowErrorToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "error");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing error toast");
            }
        }

        private async Task ShowInfoToast(string message)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("showToast", message, "info");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error showing info toast");
            }
        }

        // Metodi per analytics (opzionali)
        private async Task TrackFilterUsage(string filter)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("trackEvent", "FilterUsed", new { filter });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error tracking filter usage");
            }
        }

        private async Task TrackLoadMore()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("trackEvent", "LoadMore", new { displayCount });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error tracking load more");
            }
        }
    }
}