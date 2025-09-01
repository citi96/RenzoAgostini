using System.Linq.Expressions;
using RenzoAgostini.Server.Specifications.Interfaces;

namespace RenzoAgostini.Server.Specifications
{
    public class AndSpecification<T> : ISpecification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();

            var parameter = Expression.Parameter(typeof(T));
            var leftBody = new ReplaceParameterVisitor(leftExpression.Parameters[0], parameter)
                .Visit(leftExpression.Body);
            var rightBody = new ReplaceParameterVisitor(rightExpression.Parameters[0], parameter)
                .Visit(rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(leftBody!, rightBody!), parameter);
        }
    }

    internal class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}
