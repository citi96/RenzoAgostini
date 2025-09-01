using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;

namespace RenzoAgostini.Server.Repositories
{
    public class PaintingRepository(RenzoAgostiniDbContext context) : IPaintingRepository
    {
        public async Task<IEnumerable<Painting>> GetAllAsync()
        {
            return await context.Paintings
                .Include(p => p.Images)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Painting?> GetByIdAsync(int id)
        {
            return await context.Paintings
                .Include(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Painting?> GetBySlugAsync(string slug)
        {
            return await context.Paintings
                .Include(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<Painting> CreateAsync(Painting painting)
        {
            context.Paintings.Add(painting);
            await context.SaveChangesAsync();
            return painting;
        }

        public async Task<Painting> UpdateAsync(Painting painting)
        {
            context.Paintings.Update(painting);
            await context.SaveChangesAsync();
            return painting;
        }

        public async Task DeleteAsync(int id)
        {
            var painting = await context.Paintings.FindAsync(id);
            if (painting != null)
            {
                context.Paintings.Remove(painting);
                await context.SaveChangesAsync();
            }
        }
    }
}
