using LibraryAPI.Entities;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Services
{
    public interface IServicesUser
    {
        Task<User?> GetUser();
    }
}