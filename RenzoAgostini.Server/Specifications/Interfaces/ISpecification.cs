using System.Linq.Expressions;

namespace RenzoAgostini.Server.Specifications.Interfaces
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> ToExpression();
    }
}
