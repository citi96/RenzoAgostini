﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Entities;

namespace RenzoAgostini.Server.Data
{
    public class RenzoAgostiniDbContext(DbContextOptions<RenzoAgostiniDbContext> opt) : IdentityDbContext<ApplicationUser>(opt)
    {
        public DbSet<Painting> Paintings => Set<Painting>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<CustomOrder> CustomOrders => Set<CustomOrder>();

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

                entity.Property(p => p.Dimensions)
                    .HasMaxLength(50);

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

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CustomerFirstName).IsRequired().HasMaxLength(100);
                entity.Property(o => o.CustomerLastName).IsRequired().HasMaxLength(100);
                entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(150);
                entity.Property(o => o.AddressLine).IsRequired().HasMaxLength(200);
                entity.Property(o => o.City).IsRequired().HasMaxLength(100);
                entity.Property(o => o.PostalCode).IsRequired().HasMaxLength(20);
                entity.Property(o => o.Country).IsRequired().HasMaxLength(50);
                entity.Property(o => o.TermsAccepted).IsRequired();
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(o => o.Status).HasConversion<int>(); // memorizza enum come int
                entity.Property(o => o.StripeSessionId).HasMaxLength(200);
                entity.HasMany(o => o.Items)
                      .WithOne(i => i.Order)
                      .HasForeignKey(i => i.OrderId);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Price).HasColumnType("decimal(10,2)");
                entity.HasIndex(i => new { i.OrderId, i.PaintingId }).IsUnique();
            });


            modelBuilder.Entity<CustomOrder>(entity =>
            {
                entity.HasKey(co => co.Id);

                entity.Property(co => co.CustomerEmail)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(co => co.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(co => co.AttachmentPath)
                    .HasMaxLength(500);

                entity.Property(co => co.AttachmentOriginalName)
                    .HasMaxLength(255);

                entity.Property(co => co.AccessCode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(co => co.AccessCode)
                    .IsUnique();

                entity.Property(co => co.Status)
                    .HasConversion<int>();

                entity.Property(co => co.QuotedPrice)
                    .HasColumnType("decimal(10,2)");

                entity.Property(co => co.ArtistNotes)
                    .HasMaxLength(1000);

                // Relazione opzionale con Painting
                entity.HasOne(co => co.Painting)
                    .WithMany()
                    .HasForeignKey(co => co.PaintingId)
                    .OnDelete(DeleteBehavior.SetNull);
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
