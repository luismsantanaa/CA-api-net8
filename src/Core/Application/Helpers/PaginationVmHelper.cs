using Persistence.Pagination;
using Persistence.Specification;

namespace Application.Helpers
{
    /// <summary>
    /// Helper class for creating PaginationVm instances with common calculations.
    /// </summary>
    public static class PaginationVmHelper
    {
        /// <summary>
        /// Creates a PaginationVm with calculated PageCount from total records and page size.
        /// </summary>
        /// <typeparam name="TVm">The view model type</typeparam>
        /// <param name="totalRecords">Total number of records matching the criteria</param>
        /// <param name="specificationParams">Specification parameters containing pagination info</param>
        /// <param name="data">The paginated data</param>
        /// <returns>PaginationVm with calculated PageCount</returns>
        public static PaginationVm<TVm> Create<TVm>(
            int totalRecords,
            SpecificationParams specificationParams,
            IReadOnlyList<TVm> data)
            where TVm : class
        {
            var pageCount = CalculatePageCount(totalRecords, specificationParams.PageSize);

            return new PaginationVm<TVm>
            {
                Count = totalRecords,
                Data = data,
                PageCount = pageCount,
                PageIndex = specificationParams.PageIndex,
                PageSize = specificationParams.PageSize
            };
        }

        /// <summary>
        /// Creates a PaginationVm with explicit parameters.
        /// </summary>
        /// <typeparam name="TVm">The view model type</typeparam>
        /// <param name="totalRecords">Total number of records</param>
        /// <param name="pageIndex">Current page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="data">The paginated data</param>
        /// <returns>PaginationVm with calculated PageCount</returns>
        public static PaginationVm<TVm> Create<TVm>(
            int totalRecords,
            int pageIndex,
            int pageSize,
            IReadOnlyList<TVm> data)
            where TVm : class
        {
            var pageCount = CalculatePageCount(totalRecords, pageSize);

            return new PaginationVm<TVm>
            {
                Count = totalRecords,
                Data = data,
                PageCount = pageCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Calculates the total number of pages based on total records and page size.
        /// </summary>
        /// <param name="totalRecords">Total number of records</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Total number of pages (rounded up)</returns>
        private static int CalculatePageCount(int totalRecords, int pageSize)
        {
            if (pageSize <= 0)
                return 0;

            var rounded = Math.Ceiling(Convert.ToDecimal(totalRecords) / Convert.ToDecimal(pageSize));
            return Convert.ToInt32(rounded);
        }
    }
}

