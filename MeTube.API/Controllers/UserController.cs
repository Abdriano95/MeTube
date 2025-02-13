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

        // This method retrieves a user by their ID.
        // If the user is not found, it returns a 404 Not Found response with a message.
        // If the user is found, it maps the user entity to a UserDto and returns it with a 200 OK response.
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

        // This method retrieves all users.
        // Only accessible by Admins.
        [Authorize(Roles = "Admin")]
        [HttpGet("manageUsers")]
        public async Task<IActionResult> GetAllusers()
        {
            // Retrieve all users from the repository
            var users = await _unitOfWork.Users.GetAllAsync();

            // Check if the users list is empty
            if (!users.Any())
                return NotFound(new { Message = "Users not found" });

            // Map the user entities to ManageUserDto objects
            var userDtos = _mapper.Map<IEnumerable<ManageUserDto>>(users);

            // Return the list of user DTOs with a 200 OK response
            return Ok(userDtos);
        }

        // This method retrieves detailed information of all users.
        // Only accessible by Admins.
        [Authorize(Roles = "Admin")]
        [HttpGet("manageUsersDetails")]
        public async Task<IActionResult> GetAllUsersDetails()
        {
            // Retrieve all users from the repository
            var users = await _unitOfWork.Users.GetAllAsync();

            // Check if the users list is empty
            if (!users.Any())
                return NotFound(new { Message = "Users not found" });

            var userDtos = _mapper.Map<IEnumerable<UserDetailsDto>>(users);

            return Ok(userDtos);
        }

        // This method retrieves a user ID by their email.
        [HttpGet("userIdFromEmail")]
        public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
        {
            // Retrieve the user ID from the repository using the provided email.
            var user = await _unitOfWork.Users.GetUserIdByEmailAsync(email);

            // If the user is not found, return a 404 Not Found response with a message.
            if (user == null)
                return NotFound(new { Message = "User not found" });

            // Create a UserIdDto object with the retrieved user ID.
            var userIdDto = new UserIdDto
            {
                Id = user.Value
            };

            // Return the user ID with a 200 OK response.
            return Ok(userIdDto);
        }

        // This method handles user signup.
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] CreateUserDto request)
        {
            // Check if the model state is valid.
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

            // Map the CreateUserDto to a User entity.
            var user = _mapper.Map<User>(request);

            // Add the new user to the repository and save changes.
            await _unitOfWork.Users.AddUserAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User signed up successfully" });
        }

        // This method updates a user by their ID.
        // Only accessible by Admins.
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the ID of the user making the request
            var requestFromUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userIdRequestedFromuser = int.Parse(requestFromUser);

            // Prevent the user from updating their own account
            if (userIdRequestedFromuser == id)
                return NotFound(new { Message = "Cant update your own user" });

            // Retrieve the user to be updated from the repository
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Map the update request to the user entity
            _mapper.Map(request, user);

            // Save changes to the repository
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User updated successfully" });
        }

        // This method deletes a user by their ID.
        // Only accessible by Admins.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Get the ID of the user making the request
            var requestFromUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userIdRequestedFromuser = int.Parse(requestFromUser);

            // Prevent the user from deleting their own account
            if (userIdRequestedFromuser == id)
                return NotFound(new { Message = "Cant delete your own user" });

            // Retrieve the user to be deleted from the repository
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            // Delete the user from the repository
            await _unitOfWork.Users.DeleteUser(user);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { Message = "User deleted" });
        }

        // This method handles user login.
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Retrieve the user by username from the repository
                var user = await _unitOfWork.Users.GetUserByUsernameAsync(request.Username);
                if (user == null || user.Password != request.Password)
                {
                    return BadRequest(new { Message = "Invalid username or password" });
                }

                // Generate a JWT token for the user
                var token = GenerateJwtToken(user);

                // Map the user entity to a UserDto
                var userDto = _mapper.Map<UserDto>(user);

                // Return the user DTO and token with a 200 OK response
                return Ok(new
                {
                    User = userDto,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error response with an error message
                return StatusCode(500, new { Error = "Could not log in", Message = ex.Message });
            }
        }

        // This method handles user logout.
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                // Retrieve the token from the request headers
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Check if the token is provided
                if (string.IsNullOrEmpty(token))
                    return BadRequest(new { Message = "Token is required." });

                // Return a success message
                return Ok(new { Message = "User successfully logged out." });
            }
            catch (Exception ex)
            {
                // Return a 500 Internal Server Error response with an error message
                return StatusCode(500, new { Error = "Could not log out", Message = ex.Message });
            }
        }

        // This method generates a JWT token for a user.
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

        // This method checks if a user exists by their username or email.
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
        // This method retrieves the username of the logged-in user
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


