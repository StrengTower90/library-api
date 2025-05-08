using Microsoft.AspNetCore.Mvc.Filters;

namespace LibraryAPI.Utilities
{
    public class MyActionFilter : IActionFilter
    {
        private readonly ILogger<MyActionFilter> logger;

        public MyActionFilter(ILogger<MyActionFilter> logger)
        {
            this.logger = logger;
        }

        // It execute before the action
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Executing the action");
        }

        // It execute after the action
        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("Action Executed");
        }
    }
}
