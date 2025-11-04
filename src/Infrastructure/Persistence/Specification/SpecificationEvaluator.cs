using Domain.Base;
using Microsoft.EntityFrameworkCore;
using Persistence.Specification.Contracts;

namespace Persistence.Specification
{
    public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity, new()
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
        {
            if (specification.Criteria is not null)
            {
                inputQuery = inputQuery.Where(specification.Criteria!);
            }

            if (specification.OrderBy is not null)
            {
                inputQuery = inputQuery.OrderBy(specification.OrderBy!);
            }

            if (specification.OrderByDescending is not null)
            {
                inputQuery = inputQuery.OrderByDescending(specification.OrderByDescending!);
            }

            if (specification.IsPagingEnable)
            {
                inputQuery = inputQuery.Skip(specification.Skip).Take(specification.Take);
            }

            if (specification.Includes!.Any())
            {
                inputQuery = specification.Includes!
                                           .Aggregate(inputQuery, (current, include) =>
                                               current.Include(include)
                                           );
            }

            return inputQuery;

        }
    }
}
