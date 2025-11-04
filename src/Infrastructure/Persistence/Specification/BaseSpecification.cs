using System.Linq.Expressions;
using Persistence.Specification.Contracts;

namespace Persistence.Specification
{
    /// <summary>
    /// Base class for all specifications providing common functionality.
    /// Handles criteria, includes, ordering, and pagination in a reusable way.
    /// </summary>
    /// <typeparam name="T">The entity type for the specification</typeparam>
    public class BaseSpecification<T> : ISpecification<T>
    {
        public BaseSpecification() { }
        
        public BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        public Expression<Func<T, bool>>? Criteria { get; }

        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        public Expression<Func<T, object>>? OrderBy { get; private set; }

        public Expression<Func<T, object>>? OrderByDescending { get; private set; }

        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPagingEnable { get; private set; }

        /// <summary>
        /// Adds an order by expression for ascending order.
        /// </summary>
        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        /// <summary>
        /// Adds an order by expression for descending order.
        /// </summary>
        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        /// <summary>
        /// Applies pagination to the specification.
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnable = true;
        }

        /// <summary>
        /// Applies pagination from SpecificationParams.
        /// Automatically calculates skip based on PageIndex and PageSize.
        /// </summary>
        /// <param name="params">The specification parameters containing pagination info</param>
        protected void ApplyPaging(SpecificationParams @params)
        {
            var skip = @params.PageSize * (@params.PageIndex - 1);
            ApplyPaging(skip, @params.PageSize);
        }

        /// <summary>
        /// Adds an include expression to load related entities.
        /// </summary>
        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Applies sorting based on a sort string using a dictionary mapping sort values to expressions.
        /// Provides default sorting if sort string is empty or invalid.
        /// </summary>
        /// <param name="sort">The sort string from parameters (e.g., "nameAsc", "priceDesc")</param>
        /// <param name="sortMappings">Dictionary mapping sort strings to order expressions. 
        /// Keys should follow pattern "fieldAsc" or "fieldDesc" for ascending/descending</param>
        /// <param name="defaultOrderBy">Default order expression if sort is empty or invalid</param>
        /// <param name="defaultOrderDescending">If true, default order is descending; otherwise ascending</param>
        protected void ApplySorting(
            string? sort,
            Dictionary<string, Expression<Func<T, object>>> sortMappings,
            Expression<Func<T, object>> defaultOrderBy,
            bool defaultOrderDescending = true)
        {
            if (!string.IsNullOrWhiteSpace(sort) && sortMappings.TryGetValue(sort, out var orderExpression))
            {
                // Determine if ascending or descending based on sort string ending
                if (sort.EndsWith("Desc", StringComparison.OrdinalIgnoreCase))
                {
                    AddOrderByDescending(orderExpression);
                }
                else
                {
                    AddOrderBy(orderExpression);
                }
            }
            else
            {
                // Apply default sorting
                if (defaultOrderDescending)
                {
                    AddOrderByDescending(defaultOrderBy);
                }
                else
                {
                    AddOrderBy(defaultOrderBy);
                }
            }
        }

        /// <summary>
        /// Creates a Contains filter expression for string properties.
        /// Returns true if search term is empty/null, otherwise checks if property contains the search term.
        /// </summary>
        /// <param name="searchTerm">The search term to filter by</param>
        /// <param name="propertySelector">Expression selecting the property to search</param>
        /// <returns>Expression that returns true if searchTerm is empty, otherwise checks Contains</returns>
        protected static Expression<Func<T, bool>> ContainsFilter(
            string? searchTerm,
            Expression<Func<T, string?>> propertySelector)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return x => true; // No filter if search term is empty
            }

            // Build Contains expression dynamically
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            var constant = Expression.Constant(searchTerm, typeof(string));
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var containsCall = Expression.Call(property, containsMethod, constant);
            
            return Expression.Lambda<Func<T, bool>>(containsCall, parameter);
        }

        /// <summary>
        /// Combines multiple filter expressions with AND logic.
        /// </summary>
        /// <param name="expressions">Filter expressions to combine</param>
        /// <returns>Combined expression using AND logic</returns>
        protected static Expression<Func<T, bool>> CombineFilters(
            params Expression<Func<T, bool>>[] expressions)
        {
            if (expressions == null || expressions.Length == 0)
            {
                return x => true;
            }

            if (expressions.Length == 1)
            {
                return expressions[0];
            }

            // Combine expressions using AND (&&)
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combined = null;

            foreach (var expr in expressions)
            {
                var body = new ParameterReplacer(expr.Parameters[0], parameter).Visit(expr.Body);
                combined = combined == null ? body : Expression.AndAlso(combined, body);
            }

            return Expression.Lambda<Func<T, bool>>(combined!, parameter);
        }

        /// <summary>
        /// Helper class to replace parameters in expressions when combining them.
        /// </summary>
        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
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
}
