using LibraryAPI.DTOs;

namespace LibraryAPI.Services.v1
{
    public interface IServiceAuthors
    {
        Task<IEnumerable<AuthorDTO>> Get(PaginationDTO paginationDTO);
    }
}