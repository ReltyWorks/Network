using Auth.Jwt;
using Auth.Entities;
using Auth.Repositories;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            var user = _userRepository.GetByUserName(dto.id);

            if (user == null)
                return Unauthorized();

            if (user.Password.Equals(dto.pw) == false)
                return Unauthorized();

            Guid sessionId = Guid.NewGuid();
            _loginSessions.Add(sessionId, user);
            var jwt = JwtUtils.Generate(user.Id.ToString(), sessionId.ToString(), TimeSpan.FromHours(1));
            return Ok(new { jwt });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutDTO dto)
        {
            var user = _userRepository.GetByUserName(dto.id);

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
