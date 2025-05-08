using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace LibraryAPI.Utilities
{
    public class FilterTimeExecution : IAsyncActionFilter
    {
        private readonly ILogger<FilterTimeExecution> logger;

        public FilterTimeExecution(ILogger<FilterTimeExecution> logger)
        {
            this.logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Before the action execution
            var stopWatch = Stopwatch.StartNew();
            logger.LogInformation($"Start Action: {context.ActionDescriptor.DisplayName}");

            await next();

            // After the action execution
            stopWatch.Stop();
            logger.LogInformation($"End Action: {context.ActionDescriptor.DisplayName} - Time: {stopWatch.ElapsedMilliseconds} ms");
        }
    }
}
