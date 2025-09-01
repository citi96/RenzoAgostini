using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Models;

namespace RenzoAgostini.Data
{
    public class RenzoAgostiniDbContext(DbContextOptions<RenzoAgostiniDbContext> opt) : DbContext(opt)
    {
        public DbSet<Painting> Paintings => Set<Painting>();
    }
}
