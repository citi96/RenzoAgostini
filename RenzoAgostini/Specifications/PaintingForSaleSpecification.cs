using System.Linq.Expressions;
using RenzoAgostini.Entities;
using RenzoAgostini.Specifications.Interfaces;

namespace RenzoAgostini.Specifications
{
    public class PaintingForSaleSpecification : ISpecification<Painting>
    {
        public Expression<Func<Painting, bool>> ToExpression()
        {
            return painting => painting.IsForSale;
        }
    }
}
