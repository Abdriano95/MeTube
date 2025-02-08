using AutoMapper;
using MeTube.Data.Repository;
using MeTube.DTO;
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
    }
}
