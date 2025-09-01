namespace RenzoAgostini.Server.Entities
{
    public class PaintingImage
    {
        public string Url { get; set; } = string.Empty;
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool IsPrimary { get; set; }

    }
}
