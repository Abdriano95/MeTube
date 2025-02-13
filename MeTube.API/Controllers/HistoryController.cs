using AutoMapper;
using MeTube.Data.Entity;
using MeTube.Data.Repository;
using MeTube.DTO.HistoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MeTube.API.Controllers
{
    /// <summary>
    /// Controller for managing user history records.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryController"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work instance for data access.</param>
        /// <param name="mapper">The AutoMapper instance for mapping between entities and DTOs.</param>
        public HistoryController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all history records for the authenticated user.
        /// </summary>
        /// <returns>An IActionResult containing a collection of <see cref="HistoryDto"/> objects.</returns>
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

        /// <summary>
        /// Adds a new history record for the authenticated user.
        /// </summary>
        /// <param name="historyDto">The history data transfer object.</param>
        /// <returns>An IActionResult indicating the result of the add operation.</returns>
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

        /// <summary>
        /// Retrieves history records for a specific user (Admin only).
        /// </summary>
        /// <param name="userId">The user ID for which to retrieve history records.</param>
        /// <returns>An IActionResult containing a collection of <see cref="HistoryAdminDto"/> objects.</returns>
        [HttpGet("admin/user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistoryForUser(int userId)
        {
            if (userId == 0)
                return BadRequest(new { Message = "Invalid user id" });
            else if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

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

        /// <summary>
        /// Creates a new history record for a specified user (Admin only).
        /// </summary>
        /// <param name="historyCreateDto">The DTO containing the details for the history record to be created.</param>
        /// <returns>An IActionResult with the created history record as a <see cref="HistoryAdminDto"/>.</returns>
        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHistory([FromBody] HistoryCreateDto historyCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!User.IsInRole("Admin"))
                return Forbid();
            else if (historyCreateDto.UserId == 0)
                return BadRequest(new { Message = "Invalid user id" });

            var user = await _unitOfWork.Users.GetUserByIdAsync(historyCreateDto.UserId);
            if (user == null)
                return NotFound(new { Message = "User not found" });

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

        /// <summary>
        /// Updates an existing history record (Admin only).
        /// </summary>
        /// <param name="historyId">The ID of the history record to update.</param>
        /// <param name="historyUpdateDto">The DTO containing the updated history details.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        [HttpPut("admin/{historyId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHistory(int historyId, [FromBody] HistoryUpdateDto historyUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            else if (!User.IsInRole("Admin"))
                return Forbid();
            else if (historyId != historyUpdateDto.Id)
                return BadRequest(new { Message = "Id mismatch" });
            else if (historyUpdateDto.UserId == 0)
                return BadRequest(new { Message = "Invalid user id" });

            var existing = await _unitOfWork.Histories.GetHistoryWithRelationsAsync(historyId);
            if (existing == null)
                return NotFound(new { Message = "History not found" });

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

        /// <summary>
        /// Deletes a history record (Admin only).
        /// </summary>
        /// <param name="historyId">The ID of the history record to delete.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        [HttpDelete("admin/{historyId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHistory(int historyId)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();
            var existing = await _unitOfWork.Histories.GetHistoryWithRelationsAsync(historyId);
            if (existing == null)
                return NotFound(new { Message = "History not found" });
            else if (existing.UserId == 0)
                return BadRequest(new { Message = "Invalid user id" });

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
