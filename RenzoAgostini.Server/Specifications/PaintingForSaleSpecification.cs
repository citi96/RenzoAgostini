using System.Linq.Expressions;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Specifications.Interfaces;

namespace RenzoAgostini.Server.Specifications
{
    public class PaintingForSaleSpecification : ISpecification<Painting>
    {
        public Expression<Func<Painting, bool>> ToExpression()
        {
            return painting => painting.IsForSale;
        }
    }
}
