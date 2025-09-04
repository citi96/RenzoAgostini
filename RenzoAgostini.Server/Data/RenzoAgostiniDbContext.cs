using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Data
{
    public class RenzoAgostiniDbContext(DbContextOptions<RenzoAgostiniDbContext> opt) : IdentityDbContext<ApplicationUser>(opt)
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
                new Painting {
                    Id = 1,
                    Slug = "alba-sul-mare",
                    Title =  "Alba sul mare",
                    Description = "Serie marina. Colori caldi.",
                    Year = 2023,
                    Medium = "Olio su tela",
                    Price = 1200m,
                    IsForSale = true,
                    Images = [
                        new PaintingImage {
                            Url = "/img/q1a.jpg",
                            Width = 800,
                            Height = 600,
                            IsPrimary = true
                        },
                        new PaintingImage {
                            Url = "/img/q1b.jpg",
                            Width = 800,
                            Height = 600,
                            IsPrimary = false
                        }
                    ]
                },
                new Painting {
                    Id = 2,
                    Slug = "notturno",
                    Title =  "Notturno",
                    Description = "Acrilico, toni blu.",
                    Year = 2021,
                    Medium = "Acrilico su tavola",
                    Price = null,
                    IsForSale = false,
                    Images = [
                        new PaintingImage {
                            Url = "/img/q2a.jpg", 
                            Width = 800, 
                            Height = 600, 
                            IsPrimary = true
                        },
                        new PaintingImage {
                            Url = "/img/q2b.jpg", 
                            Width = 800, 
                            Height = 600, 
                            IsPrimary = false 
                        }
                    ]
                },
                new Painting {
                    Id = 3,
                    Slug = "colline",
                    Title ="Colline",
                    Description = "Paesaggio primaverile.",
                    Year = 2024,
                    Medium = "Olio su tela",
                    Price = 900m,
                    IsForSale = true,
                    Images = [
                        new PaintingImage {
                            Url = "/img/q3a.jpg",
                            Width =  800,
                            Height = 600,
                            IsPrimary = true
                        }
                    ]
                }
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
