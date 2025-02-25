using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Layer.Specification
{
    public interface ISpecification<T>
    {
        // Criteria
        Expression<Func<T, bool>> Criteria { get; }
        // Includes
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>> OrderByAsc { get; }
        Expression<Func<T, object>> OrderByDesc { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPaginated { get; }
    }
}
