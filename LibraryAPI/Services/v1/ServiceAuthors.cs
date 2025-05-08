using AutoMapper;
using LibraryAPI.Data;
using LibraryAPI.DTOs;
using LibraryAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Services.v1
{
    public class ServiceAuthors : IServiceAuthors
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public ServiceAuthors(ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AuthorDTO>> Get(PaginationDTO paginationDTO)
        {
            var queryable = context.Authors.AsQueryable(); // to build step by step the query on memory
            await httpContextAccessor.HttpContext!.InsertPaginationParamsInHeader(queryable);
            var authors = await queryable
                                .OrderBy(x => x.Names)
                                .Page(paginationDTO).ToListAsync();
            var authorsDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return authorsDTO;
        }
    }
}
