using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using LibraryAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/books")]
    [Authorize(Policy = "esadmin")]
    public class BooksControllers : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly ITimeLimitedDataProtector protectionToTimeLimit;
        private const string cache = "libros-obtener";

        public BooksControllers(ApplicationDbContext context, IMapper mapper, 
            IDataProtectionProvider protectionProvider, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            protectionToTimeLimit = protectionProvider
                .CreateProtector("BooksControllers").ToTimeLimitedDataProtector();
        }

        [HttpGet("list/retrieve-token", Name = "RetrieveTokenV1")]
        [OutputCache(Tags = [cache])]
        public ActionResult RetrieveToken()
        {
            var plainText = Guid.NewGuid().ToString();
            var token = protectionToTimeLimit.Protect(plainText, lifetime: TimeSpan.FromSeconds(30));
            var url = Url.RouteUrl("RetrieveBooksListWithTokenV1", new { token }, "https");
            return Ok(new { url });
        }

        [HttpGet("list/{token}", Name = "RetrieveBooksListWithTokenV1")]
        [AllowAnonymous]
        public async Task<ActionResult> RetrieveBooksListWithToken(string token)
        {
            try
            {
                protectionToTimeLimit.Unprotect(token);
            }
            catch
            {
                ModelState.AddModelError(nameof(token), "The token has expired");
                return ValidationProblem();
            }

            var books = await context.Books.ToListAsync();
            var booksDTO = mapper.Map<IEnumerable<BookDTO>>(books);
            return Ok(booksDTO);
        }

        [HttpGet(Name = "GetBooksV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<BookDTO>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            //var books = await context.Books.ToListAsync();
            var queryable = context.Books.AsQueryable();
            await HttpContext.InsertPaginationParamsInHeader(queryable);
            var books = await queryable
                    .OrderBy(x => x.Title)
                    .Page(paginationDTO).ToListAsync();
            var booksDTO = mapper.Map<IEnumerable<BookDTO>>(books);
            return booksDTO;
        }

        [HttpGet("{id:int}", Name = "RetrieveBookV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<BookWithAuthorDTO>> Get(int id)
        {
            var book = await context.Books
                .Include(x => x.Authors)
                    .ThenInclude(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (book is null)
            {
                return NotFound();
            }

            var bookDTO = mapper.Map<BookWithAuthorDTO>(book);

            return bookDTO;
        }

        [HttpPost(Name = "CreateBookV1")]
        [ServiceFilter<FilterBookValidation>()]
        public async Task<ActionResult> Post(BookCreationDTO bookCreationDTO)
        {
            //if (bookCreationDTO.AuthorIds is null || bookCreationDTO.AuthorIds.Count == 0)
            //{
            //    ModelState.AddModelError(nameof(bookCreationDTO.AuthorIds),
            //        "Does not create a book without authors");
            //    return ValidationProblem();
            //}

            //var authorThatExist = await context.Authors
            //                      .Where(x => bookCreationDTO.AuthorIds.Contains(x.Id))
            //                      .Select(x => x.Id).ToListAsync();

            //if (authorThatExist.Count != bookCreationDTO.AuthorIds.Count)
            //{
            //    var authorsNotExist = bookCreationDTO.AuthorIds.Except(authorThatExist);
            //    var authorsNotExistString = string.Join(",", authorsNotExist);
            //    var messageError = $"The next authors don't exist: {authorsNotExistString}";
            //    ModelState.AddModelError(nameof(bookCreationDTO.AuthorIds), messageError);
            //    return ValidationProblem();
            //}


            var book = mapper.Map<Book>(bookCreationDTO);
            AssingAuthorsOrder(book);

            context.Add(book);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            var bookDTO = mapper.Map<BookDTO>(book);

            return CreatedAtRoute("RetrieveBookV1", new { book.Id }, bookDTO);
        }

        private void AssingAuthorsOrder(Book book)
        {
            if (book.Authors is not null)
            {
                for (int i = 0; i < book.Authors.Count; i++)
                {
                    book.Authors[i].Order = i;
                }
            }
        }

        [HttpPut("{id:int}", Name = "UpdateBookV1")]
        [ServiceFilter<FilterBookValidation>()]
        public async Task<ActionResult> Update(int id, BookCreationDTO bookCreationDTO)
        {

            //if (bookCreationDTO.AuthorIds is null || bookCreationDTO.AuthorIds.Count == 0)
            //{
            //    ModelState.AddModelError(nameof(bookCreationDTO.AuthorIds),
            //        "Does not create a book without authors");
            //    return ValidationProblem();
            //}

            //var authorThatExist = await context.Authors
            //                      .Where(x => bookCreationDTO.AuthorIds.Contains(x.Id))
            //                      .Select(x => x.Id).ToListAsync();

            //if (authorThatExist.Count != bookCreationDTO.AuthorIds.Count)
            //{
            //    var authorsNotExist = bookCreationDTO.AuthorIds.Except(authorThatExist);
            //    var authorsNotExistString = string.Join(",", authorsNotExist);
            //    var messageError = $"The next authors don't exist: {authorsNotExistString}";
            //    ModelState.AddModelError(nameof(bookCreationDTO.AuthorIds), messageError);
            //    return ValidationProblem();
            //}

            //var book = mapper.Map<Book>(bookCreationDTO);
            //book.Id = id;

            //context.Update(book);

            var bookDB = await context.Books
                            .Include(x => x.Authors)
                            .FirstOrDefaultAsync(x => x.Id == id);

            if (bookDB is null)
            {
                return NotFound();
            }

            bookDB = mapper.Map(bookCreationDTO, bookDB);
            AssingAuthorsOrder(bookDB);

            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }


        [HttpDelete("{id:int}", Name = "DeleteBookV1")]
        public async Task<ActionResult> Delete(int id)
        {
            var deletedRecords = await context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (deletedRecords == 0)
            {
                return NotFound();
            }

            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }
    }
}
