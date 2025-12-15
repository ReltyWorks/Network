using Auth.Entities;
using Auth.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : Controller
    {
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private readonly IUserRepository _userRepository;

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                IEnumerable<UserResponseDTO> response = _userRepository.GetAll()
                                                                   .Select(user => new UserResponseDTO
                                                                   (
                                                                       id: user.Id,
                                                                       username: user.Username,
                                                                       nickname: user.Nickname,
                                                                       lastConnected: user.LastConnected
                                                                   ));
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }
        }

        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] CreateUserDTO dto)
        {
            if (string.IsNullOrEmpty(dto.username))
                return BadRequest(new { error = "Username is necessary." });

            if (string.IsNullOrEmpty(dto.password))
                return BadRequest(new { error = "Password is necessary." });

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.username,
                Password = dto.password,
                Nickname = null,
                CreatedAt = DateTime.UtcNow,
                LastConnected = DateTime.UtcNow,
            };

            try
            {
                var users = _userRepository.GetAll();
                bool exist = users.Any(user => user.Username.Equals(dto.username)); // DB 에 이미 id 등록된거있는지

                if (exist)
                {
                    return Conflict(new { error = "Already registered username." });
                }
                else
                {
                    _userRepository.Insert(user);
                    _userRepository.Save();

                    var response = new UserResponseDTO(
                            id: user.Id,
                            username: user.Username,
                            nickname: user.Nickname,
                            lastConnected: user.LastConnected
                        );

                    return CreatedAtAction("Create", response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }
        }

        [HttpPatch("{id:guid}/nickname")]
        public IActionResult UpdateNickname(Guid id, [FromBody] UpdateNicknameDTO dto)
        {
            var user = _userRepository.GetById(id);

            if (user == null)
                return NotFound(new { error = "User not found" });

            try
            {
                var users = _userRepository.GetAll();
                var exist = users.Any(user => user.Nickname.Equals(dto.nickname, StringComparison.OrdinalIgnoreCase));

                if (exist)
                {
                    return Conflict(new { isExist = exist, message = "Nickname already exist. " });
                }
                else
                {
                    user.Nickname = dto.nickname;
                    _userRepository.Update(user);
                    return Ok(new { isExist = exist, message = "Updated nickname.", newNickname = user.Nickname });

                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { error = "Server error", details = ex.Message });
            }
        }
    }

    public record UserResponseDTO(Guid id, string username, string nickname, DateTime lastConnected);
    public record CreateUserDTO(string username, string password);

    public record UpdateNicknameDTO(string nickname);
}
