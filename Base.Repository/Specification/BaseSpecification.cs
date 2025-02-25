using System.Linq.Expressions;

namespace Repository.Layer.Specification
{
    public class BaseSpecifications<T> : ISpecification<T>
    {
        public BaseSpecifications(Expression<Func<T, bool>> criteria)
        {
            this.Criteria = criteria;
        }
        public Expression<Func<T, bool>> Criteria { get; }

        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        public Expression<Func<T, object>> OrderByAsc { get; private set; }

        public Expression<Func<T, object>> OrderByDesc { get; private set; }

        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPaginated { get; private set; }

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            this.Includes.Add(includeExpression);
        }
        protected void AddOrderByAsc(Expression<Func<T, object>> orderByExpression)
        {
            this.OrderByAsc = orderByExpression;
        }
        protected void AddOrderByDesc(Expression<Func<T, object>> orderByExpression)
        {
            this.OrderByDesc = orderByExpression;
        }
        protected void ApplyPaging(int skip, int take)
        {
            this.Skip = skip;
            this.Take = take;
            this.IsPaginated = true;
        }
    }
}
