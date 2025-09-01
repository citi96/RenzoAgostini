using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Entities;

namespace RenzoAgostini.Data
{
    public class RenzoAgostiniDbContext(DbContextOptions<RenzoAgostiniDbContext> opt) : DbContext(opt)
    {
        public DbSet<Painting> Paintings => Set<Painting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurazione Painting
            modelBuilder.Entity<Painting>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Slug)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(p => p.Slug)
                    .IsUnique();

                entity.Property(p => p.Description)
                    .HasMaxLength(1000);

                entity.Property(p => p.Medium)
                    .HasMaxLength(100);

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(10,2)");

                // Configurazione Images come owned entities
                entity.OwnsMany(p => p.Images, img =>
                {
                    img.ToTable("PaintingImages");
                    img.WithOwner().HasForeignKey("PaintingId");
                    img.HasKey("PaintingId", "Url"); // Composite key

                    img.Property(i => i.Url)
                        .IsRequired()
                        .HasMaxLength(500);

                    img.Property(i => i.Width);
                    img.Property(i => i.Height);
                    img.Property(i => i.IsPrimary);
                });
            });

            // Seed data per development
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var paintings = new[]
            {
                new Painting(
                    1,
                    "alba-sul-mare",
                    "Alba sul mare",
                    "Serie marina. Colori caldi.",
                    2023,
                    "Olio su tela",
                    1200m,
                    true,
                    [
                        new("/img/q1a.jpg", 800, 600, true),
                        new("/img/q1b.jpg", 800, 600, false)
                    ]
                ),
                new Painting(
                    2,
                    "notturno",
                    "Notturno",
                    "Acrilico, toni blu.",
                    2021,
                    "Acrilico su tavola",
                    null,
                    false,
                    [
                        new("/img/q2a.jpg", 800, 600, true),
                        new("/img/q2b.jpg", 800, 600, false)
                    ]
                ),
                new Painting(
                    3,
                    "colline",
                    "Colline",
                    "Paesaggio primaverile.",
                    2024,
                    "Olio su tela",
                    900m,
                    true,
                    [
                        new("/img/q3a.jpg", 800, 600, true)
                    ]
                )
            };

            modelBuilder.Entity<Painting>().HasData(
                paintings.Select(p => new
                {
                    p.Id,
                    p.Slug,
                    p.Title,
                    p.Description,
                    p.Year,
                    p.Medium,
                    p.Price,
                    p.IsForSale
                })
            );

            // Seed delle immagini
            var images = paintings.SelectMany((p, pIndex) =>
                p.Images.Select((img, imgIndex) => new
                {
                    PaintingId = p.Id,
                    img.Url,
                    img.Width,
                    img.Height,
                    img.IsPrimary
                })
            );

            modelBuilder.Entity<Painting>()
                .OwnsMany(p => p.Images)
                .HasData(images);
        }
    }
}
