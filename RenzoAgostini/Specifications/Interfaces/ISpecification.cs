using System.Linq.Expressions;

namespace RenzoAgostini.Specifications.Interfaces
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }
}
