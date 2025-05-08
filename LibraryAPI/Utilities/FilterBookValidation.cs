using LibraryAPI.Data;
using LibraryAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Utilities
{
    public class FilterBookValidation : IAsyncActionFilter
    {
        private readonly ApplicationDbContext dbContext;

        public FilterBookValidation(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.TryGetValue("bookCreationDTO", out var value) ||
                value is not BookCreationDTO bookCreationDTO)
            {
                context.ModelState.AddModelError(string.Empty, "The sent model is not valid");
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }

            if (bookCreationDTO.AuthorIds is null || bookCreationDTO.AuthorIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(bookCreationDTO.AuthorIds),
                    "Does not create a book without authors");
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }

            var authorThatExist = await dbContext.Authors
                                  .Where(x => bookCreationDTO.AuthorIds.Contains(x.Id))
                                  .Select(x => x.Id).ToListAsync();

            if (authorThatExist.Count != bookCreationDTO.AuthorIds.Count)
            {
                var authorsNotExist = bookCreationDTO.AuthorIds.Except(authorThatExist);
                var authorsNotExistString = string.Join(",", authorsNotExist);
                var messageError = $"The next authors don't exist: {authorsNotExistString}";
                context.ModelState.AddModelError(nameof(bookCreationDTO.AuthorIds), messageError);
                context.Result = context.ModelState.BuildProblemDetail();
                return;
            }

            //If pass the validation it continue with the remain filters pipeline
            await next();
        }
    }
}
