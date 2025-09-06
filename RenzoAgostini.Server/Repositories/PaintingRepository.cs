using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Repositories.Interfaces;

namespace RenzoAgostini.Server.Repositories
{
    public class PaintingRepository(RenzoAgostiniDbContext context) : RepositoryBase<Painting>(context), IPaintingRepository
    {
        protected override IQueryable<Painting> IncludeRelated(IQueryable<Painting> query) =>
            query.Include(p => p.Images);
               
        public async Task<Painting?> GetBySlugAsync(string slug)
        {
            return await context.Paintings
                .Include(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }                    
    }
}
