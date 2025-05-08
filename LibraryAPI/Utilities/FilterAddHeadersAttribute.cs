using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities
{
    public class FilterAddHeadersAttribute: ActionFilterAttribute
    {
        private readonly string name;
        private readonly string value;

        public FilterAddHeadersAttribute(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // Before the action execution
            context.HttpContext.Response.Headers.Append(name, value);
            base.OnResultExecuting(context);
            // After the action execution
        }
    }
}
