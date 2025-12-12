using Auth.Repositories;
using Auth.Entities;
using Microsoft.AspNetCore.Mvc;

/*
 * HTTP 메서드
 * Get : 조회
 * Post : 생성
 * Put : 전체 수정
 * Patch : 부분 수정
 * Delete : 삭제
*/

namespace Auth.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : Controller
    {
        IUserRepository _userRepository;
        Dictionary<Guid, User> _loginSessions = new();

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            User user = _userRepository.GetByUserName(dto.id);

            if (user == null)
                return Unauthorized();

            if (user.Password.Equals(dto.pw) == false)
                return Unauthorized();

            _loginSessions.Add(user.Id, user);
            return Ok(); // 여기서 클라이언트에게 토큰같은 보내고 싶은걸 보낼 수 있다.
        }

        public IActionResult Logout([FromBody] LogoutDTO dto)
        {
            var user = _userRepository.GetByUserName(dto.id);

            if (user == null)
                return Unauthorized();

            if (user.Password.Equals(dto.id) == false)
                return Unauthorized();

            _loginSessions.Remove(user.Id);
            return Ok();
        }


    }

    public record LoginDTO(string id, string pw);
    public record LogoutDTO(string id);
}
