using Microsoft.AspNetCore.Mvc;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO;
using AutoMapper;
using System.Threading.Tasks;

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
            await _unitOfWork.Update();

            return Ok(new { Message = "User signed up successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _mapper.Map(request, user);

            await _unitOfWork.Update();

            return Ok(new { Message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            _unitOfWork.Users.DeleteUser(user);
            await _unitOfWork.Update();

            return Ok(new { Message = "User deleted successfully" });
        }
    }
}


