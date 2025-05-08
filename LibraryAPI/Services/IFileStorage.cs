namespace LibraryAPI.Services
{
    public interface IFileStorage
    {
        Task Delete(string? path, string container);
        Task<string> Storage(string container, IFormFile file);
        async Task<string> Edit(string? path, string container, IFormFile file)
        {
            await Delete(path, container);
            return await Storage(container, file);
        }
    }
}
