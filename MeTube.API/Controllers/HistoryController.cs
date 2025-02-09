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
    }
}
