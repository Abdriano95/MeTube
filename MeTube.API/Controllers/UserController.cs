using Microsoft.AspNetCore.Mvc;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.JSInterop;

namespace MeTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("manageUsers")]
        public async Task<IActionResult> GetAllusers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            if(!users.Any())
                return NotFound(new { Message = "Users not found" });

            var userDtos = _mapper.Map<IEnumerable<ManageUserDto>>(users);
            return Ok(userDtos);
        }

        [HttpGet("userIdFromEmail")]
        public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
        {
            var user = await _unitOfWork.Users.GetUserIdByEmailAsync(email);

            if (user == null)
                return NotFound(new { Message = "User not found" });

            var userIdDto = new UserIdDto 
            { 
                Id = user.Value 
            };

            return Ok(userIdDto);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] CreateUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _unitOfWork.Users.GetUserByUsernameAsync(request.Username) != null)
            {
                return BadRequest(new { Message = "Username already exists" });
            }

            if (await _unitOfWork.Users.GetUserByEmailAsync(request.Email) != null)
            {
                return BadRequest(new { Message = "Email already exists" });
            }

            var user = _mapper.Map<User>(request);

            await _unitOfWork.Users.AddUserAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User signed up successfully" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestFromUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userIdRequestedFromuser = int.Parse(requestFromUser);
            if (userIdRequestedFromuser == id)
                return NotFound(new { Message = "Cant delete your own user" });

            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _mapper.Map(request, user);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var requestFromUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userIdRequestedFromuser = int.Parse(requestFromUser);
            if (userIdRequestedFromuser == id)
                return NotFound(new { Message = "Cant delete your own user" });

            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _unitOfWork.Users.DeleteUser(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User deleted" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
                if (user == null || user.Password != request.Password)
                {
                    return BadRequest(new { Message = "Invalid username or password" });
                }
                var token = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);

                return Ok(new
                {
                    User = userDto,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Could not log in", Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { Message = "Token is required." });

                return Ok(new { Message = "User successfully logged out." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Could not log out", Message = ex.Message });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("VerySecretMeTubePasswordVerySecretMeTubePassword"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: "Customer",
                audience: "User",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckIfUserExists([FromQuery] string username, [FromQuery] string email)
        {
            bool usernameExists = await _unitOfWork.Users.UsernameExistsAsync(username);
            bool emailExists = await _unitOfWork.Users.EmailExistsAsync(email);

            if (!usernameExists && !emailExists)
                return Ok(new Dictionary<string, object> { { "Exists", false }, { "message", "User does not exist" } });

            var errorMessages = new List<string>();
            if (usernameExists) errorMessages.Add("Username already exists\n");
            if (emailExists) errorMessages.Add("Email already exists");

            return BadRequest(new Dictionary<string, object>
            {
                { "Exists", true },
                { "message", string.Join("", errorMessages)}
            });
        }
    }
}


