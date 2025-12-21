using System.ComponentModel.DataAnnotations;

namespace RenzoAgostini.Server.Entities
{
    public class Biography
    {
        public int Id { get; set; }

        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}
