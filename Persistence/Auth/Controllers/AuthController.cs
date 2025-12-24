using Auth.Jwt;
using Auth.Entities;
using Auth.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Auth.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : Controller
    {
        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        IUserRepository _userRepository;
        Dictionary<Guid, User> _loginSessions = new(); // <sessionId, user>

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _userRepository.GetByUserNameAsync(dto.id);

            if (user == null)
                return Unauthorized();

            if (user.Password.Equals(dto.pw) == false)
                return Unauthorized();

            Guid sessionId = Guid.NewGuid();
            _loginSessions.Add(sessionId, user);
            var jwt = JwtUtils.Generate(user.Id.ToString(), sessionId.ToString(), TimeSpan.FromHours(1));
            string userId = user.Id.ToString();
            return Ok(new { jwt, userId, user.Nickname });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDTO dto)
        {
            var user = await _userRepository.GetByUserNameAsync(dto.id);

            if (user == null)
                return Unauthorized();

            if (_loginSessions.ContainsKey(user.Id) == false)
                return Unauthorized();

            _loginSessions.Remove(user.Id);
            return Ok();
        }
    }

    public record LoginDTO(string id, string pw);
    public record LogoutDTO(string id);
}
