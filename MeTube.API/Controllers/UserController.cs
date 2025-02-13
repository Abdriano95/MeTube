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
using Azure.Core;

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

        /// <summary>
        /// This method retrieves a user by their ID.
        /// If the user is not found, it returns a 404 Not Found response with a message.
        /// If the user is found, it maps the user entity to a UserDto and returns it with a 200 OK response.
        /// </summary>
        /// <param name="id">The user ID to retrieve</param>
        /// <returns>A user DTO or a 404 Not Found response if the user does not exist</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }

        /// <summary>
        /// This method retrieves all users.
        /// Only accessible by Admins.
        /// </summary>
        /// <returns>A list of user DTOs or a 404 Not Found response if no users exist</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("manageUsers")]
        public async Task<IActionResult> GetAllusers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            if (!users.Any())
                return NotFound(new { Message = "Users not found" });

            var userDtos = _mapper.Map<IEnumerable<ManageUserDto>>(users);

            return Ok(userDtos);
        }

        /// <summary>
        /// This method retrieves detailed information of all users.
        /// Only accessible by Admins.
        /// </summary>
        /// <returns>A list of user details DTOs or a 404 Not Found response if no users exist</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("manageUsersDetails")]
        public async Task<IActionResult> GetAllUsersDetails()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            if (!users.Any())
                return NotFound(new { Message = "Users not found" });

            var userDtos = _mapper.Map<IEnumerable<UserDetailsDto>>(users);

            return Ok(userDtos);
        }

        /// <summary>
        /// This method retrieves a user ID by their email.
        /// </summary>
        /// <param name="email">The email to search for</param>
        /// <returns>A user ID DTO or a 404 Not Found response if the user does not exist</returns>
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

        /// <summary>
        /// This method handles user signup.
        /// </summary>
        /// <param name="request">The data needed to create the user</param>
        /// <returns>A success message or a validation error message</returns>
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

        /// <summary>
        /// This method updates a user by their ID.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">The user ID to update</param>
        /// <param name="request">The data used to update the user</param>
        /// <returns>A success message or a 404 Not Found response if the user does not exist</returns>
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
                return NotFound(new { Message = "Cant update your own user" });

            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _mapper.Map(request, user);

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User updated successfully" });
        }

        /// <summary>
        /// This method deletes a user by their ID.
        /// Only accessible by Admins.
        /// </summary>
        /// <param name="id">The user ID to delete</param>
        /// <returns>A success message or a 404 Not Found response if the user does not exist</returns>
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

            await _unitOfWork.Users.DeleteUser(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User deleted" });
        }

        /// <summary>
        /// This method handles user login.
        /// </summary>
        /// <param name="request">The login credentials</param>
        /// <returns>A user DTO and a JWT token on successful login</returns>
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

        /// <summary>
        /// This method handles user logout.
        /// </summary>
        /// <returns>A success message</returns>
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

        /// <summary>
        /// This method generates a JWT token for a user.
        /// </summary>
        /// <param name="user">The user for whom the token is generated</param>
        /// <returns>A JWT token for the user</returns>
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

        /// <summary>
        /// This method checks if a user exists by their username or email.
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <param name="email">The email to check</param>
        /// <returns>A response indicating whether the user exists or not</returns>
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

        /// <summary>
        /// This method retrieves the username of the logged-in user.
        /// </summary>
        /// <returns>The username of the logged-in user</returns>
        [Authorize]
        [HttpGet("logedInUsername")]
        public async Task<IActionResult> GetLogedInUsername()
        {
            string name = User.FindFirst(ClaimTypes.Name)?.Value;
            if (name == null)
                return NotFound(new { Message = "" });
            return Ok(name);
        }
    }
}
