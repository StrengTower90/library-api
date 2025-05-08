using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace LibraryAPI.Utilities
{
    public static class HttpContextExtensions
    {
        public async static Task 
            InsertPaginationParamsInHeader<T>(this HttpContext httpContext,
            IQueryable<T> queryable)
        {
            if (httpContext is null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            double amount = await queryable.CountAsync();
            httpContext.Response.Headers.Append("total-numbers-records", amount.ToString());
        }
    }
}
