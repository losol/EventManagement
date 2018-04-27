using System.Threading.Tasks;
using losol.EventManagement.Services.TalentLms.Models;
using Microsoft.Extensions.Options;
using RestSharp;

namespace losol.EventManagement.Services.TalentLms
{
    public interface ITalentLmsService
    {
        Task<User> GetUserAsync(string email);
        Task<User> CreateUser(User user);
        Task<User> CreateUserIfNotExists(User user);
        Task EnrolUserToCourse(int userId, int courseId);
        Task<string> GetCourseLink(int userId, int courseId);
    }
}
