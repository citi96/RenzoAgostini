using System.Linq.Expressions;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Specifications.Interfaces;

namespace RenzoAgostini.Server.Specifications
{
    public class PaintingByYearRangeSpecification(int fromYear, int toYear) : ISpecification<Painting>
    {
        public Expression<Func<Painting, bool>> ToExpression()
        {
            return painting => painting.Year.HasValue &&
                              painting.Year >= fromYear &&
                              painting.Year <= toYear;
        }
    }
}
