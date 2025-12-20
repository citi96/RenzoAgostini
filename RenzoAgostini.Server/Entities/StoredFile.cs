using System.ComponentModel.DataAnnotations;

namespace RenzoAgostini.Server.Entities;

public class StoredFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>(); // BLOB

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
