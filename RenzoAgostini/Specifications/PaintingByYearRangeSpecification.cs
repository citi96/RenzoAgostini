using System.Linq.Expressions;
using RenzoAgostini.Entities;
using RenzoAgostini.Specifications.Interfaces;

namespace RenzoAgostini.Specifications
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
