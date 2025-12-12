using Auth.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                IEnumerable<UserResponseDTO> response = _userRepository.GetAll()
                                                                       .Select(User => new UserResponseDTO
                                                                       (
                                                                           id: User.Id,
                                                                           username: User.Username,
                                                                           nickname: User.Nickname,
                                                                           lastConnected: User.LastConnected
                                                                       ));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }

        }
    }

    public record UserResponseDTO(Guid id, string username, string nickname, DateTime lastConnected);
}
