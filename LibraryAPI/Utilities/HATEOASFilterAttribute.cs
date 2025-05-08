using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities
{
    public class HATEOASFilterAttribute: ResultFilterAttribute
    {
        protected bool ItMustToIncludeHATEOAS(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult result || !IsSuccetedResponse(result))
            {
                return false;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("IncludeHATEOAS", out var cabecera))
            {
                return false;
            }

            return string.Equals(cabecera, "Y", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSuccetedResponse(ObjectResult result)
        {
            if (result.Value is null)
            {
                return false;
            }

            if (result.StatusCode.HasValue && !result.StatusCode.Value.ToString().StartsWith("2"))
            {
                return false;
            }

            return true;
        }


    }
}
