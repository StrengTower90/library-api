using LibraryAPI.DTOs;

namespace LibraryAPI.Services.v1
{
    public interface IGeneradorEnlaces
    {
        Task GenerarEnlaces(AuthorDTO authorDTO);

        Task<CollectionOfResourcesDTO<AuthorDTO>> GenerarEnlaces(List<AuthorDTO> authors);
    }
}

