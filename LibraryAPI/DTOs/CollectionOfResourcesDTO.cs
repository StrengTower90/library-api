namespace LibraryAPI.DTOs
{
    public class CollectionOfResourcesDTO<T>: ResourceDTO where T: ResourceDTO
    {
        public IEnumerable<T> Values { get; set; } = [];
    }
}
