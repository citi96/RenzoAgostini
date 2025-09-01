namespace RenzoAgostini.Pages
{
    public partial class Home
    {
        protected record PaintingVm(string Title, string? Description, int? Year, string? Medium, decimal? Price, bool IsForSale, IReadOnlyList<string> ImageUrls);

        protected List<PaintingVm> paintings = new()
        {
            new("Alba sul mare", "Serie marina. Colori caldi.", 2023, "Olio su tela", 1200m, true, new[]{"/img/q1a.jpg","/img/q1b.jpg"}),
            new("Notturno", "Acrilico, toni blu.", 2021, "Acrilico su tavola", null, false, new[]{"/img/q2a.jpg","/img/q2b.jpg"}),
            new("Colline", "Paesaggio primaverile.", 2024, "Olio su tela", 900m, true, new[]{"/img/q3a.jpg"})
        };
    }
}
