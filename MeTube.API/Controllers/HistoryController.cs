using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.HistoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HistoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/History
        [HttpGet]
        public async Task<IActionResult> GetAllHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();
            var history = await _unitOfWork.Histories.GetHistoriesByUserIdAsync(userId);
            if (!history.Any())
                return NotFound(new { Message = "History not found" });
            var historyDtos = _mapper.Map<IEnumerable<HistoryDto>>(history);
            return Ok(historyDtos);
        }

        // POST: api/History
        [HttpPost]
        public async Task<IActionResult> AddHistory([FromBody] HistoryDto historyDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();
            try
            {
                var history = _mapper.Map<History>(historyDto);
                history.UserId = userId;
                await _unitOfWork.Histories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { Message = "History added" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }  
        }

        // ----------------ADMIN----------------

        // GET: api/History/admin/user/{userId}
        [HttpGet("admin/user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistoryForUser(int userId)
        {
            if (userId == 0) return BadRequest(new { Message = "Invalid user id" });

            else if (!User.IsInRole("Admin")) return Forbid();

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "User not found" });

            try
            {
                var histories = await _unitOfWork.Histories.GetHistoriesByUserIdAsync(userId);
                if (histories == null || !histories.Any())
                    return NotFound(new { Message = "History not found" });

                var adminHistoryDtos = _mapper.Map<IEnumerable<HistoryAdminDto>>(histories);
                return Ok(adminHistoryDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/History/admin
        [HttpPost("admin")]
        [Authorize(Roles= "Admin")]
        public async Task<IActionResult> CreateHistory([FromBody] HistoryCreateDto historyCreateDto)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            else if (!User.IsInRole("Admin")) return Forbid();
            else if(historyCreateDto.UserId == 0) return BadRequest(new { Message = "Invalid user id" });

            var user = await _unitOfWork.Users.GetUserByIdAsync(historyCreateDto.UserId);
            if (user == null) return NotFound(new { Message = "User not found" });

            try
            {
                var history = _mapper.Map<History>(historyCreateDto);
                await _unitOfWork.Histories.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();
                var createdDto = _mapper.Map<HistoryAdminDto>(history);
                return CreatedAtAction(nameof(GetHistoryForUser), new { userId = historyCreateDto.UserId }, createdDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT: apit/History/admin/{historyId}
        [HttpPut("admin/{historyId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHistory(int historyId, [FromBody] HistoryUpdateDto historyUpdateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            else if (!User.IsInRole("Admin")) return Forbid();
            else if (historyId != historyUpdateDto.Id) return BadRequest(new { Message = "Id mismatch" });
            else if (historyUpdateDto.UserId == 0) return BadRequest(new { Message = "Invalid user id" });

            var existing = await _unitOfWork.Histories.GetHistoryWithRelationsAsync(historyId);
            if (existing == null) return NotFound(new { Message = "History not found" });

            try
            {
                _mapper.Map(historyUpdateDto, existing);
                existing.Id = historyUpdateDto.Id;
                _unitOfWork.Histories.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                return Ok(new { Message = "History updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // DELETE: api/History/admin/{historyId}
        [HttpDelete("admin/{historyId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHistory(int historyId)
        {
            if (!User.IsInRole("Admin")) return Forbid();
            var existing = await _unitOfWork.Histories.GetHistoryWithRelationsAsync(historyId);

            if (existing == null) return NotFound(new { Message = "History not found" });
            else if (existing.UserId == 0) return BadRequest(new { Message = "Invalid user id" });

            try
            {
                _unitOfWork.Histories.Delete(existing);
                await _unitOfWork.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }



    }
}
