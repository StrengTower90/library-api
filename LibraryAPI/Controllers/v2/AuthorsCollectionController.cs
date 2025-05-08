using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers.v2
{
    [ApiController]
    [Route("api/v2/authors-collection")]
    [Authorize(Policy = "esadmin")]
    public class AuthorsCollectionController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AuthorsCollectionController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{ids}", Name = "RetrieveAuthorsByIdsV2")] // api/authors-collection/1,2,3
        public async Task<ActionResult<List<AuthorWithBooksDTO>>> Get(string ids)
        {
            var idsCollection = new List<int>();

            foreach (var id in ids.Split(","))
            {
                if (int.TryParse(id, out int idInt))
                {
                    idsCollection.Add(idInt);
                }
            }

            if (!idsCollection.Any())
            {
                ModelState.AddModelError(nameof(ids), "No ids found");
                return ValidationProblem();
            }

            var authors = await context.Authors
                            .Include(x => x.Books)
                                .ThenInclude(x => x.Book)
                            .Where(x => idsCollection.Contains(x.Id))
                            .ToListAsync();

            if (authors.Count != idsCollection.Count)
            {
                return NotFound();
            }

            var authorsDTO = mapper.Map<List<AuthorWithBooksDTO>>(authors);
            return authorsDTO;
        }


        [HttpPost]
        public async Task<ActionResult> Post(IEnumerable<AuthorCreationDTO> authorsCreationDTO)
        {
            var authors = mapper.Map<IEnumerable<Author>>(authorsCreationDTO);
            context.AddRange(authors);
            await context.SaveChangesAsync();

            var authorsDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
            var ids = authors.Select(x => x.Id);
            var idsString = string.Join(",", ids);
            return CreatedAtRoute("RetrieveAuthorsByIdsV2", new { ids = idsString }, authorsDTO);
        }
    }
}
