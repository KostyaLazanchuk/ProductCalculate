using Lab3.Models;
using Lab3.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab3.Controllers
{
    [ApiController]
    [Route("compute")]
    public class ComputeController(ComputeQueue queue) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Enqueue([FromBody] ComputeDto dto, CancellationToken ct)
        {
            if (dto.N < 2 || dto.N > 200_000_000)
                return BadRequest("N must be between 2 and 200,000,000.");

            var jobId = Guid.NewGuid();
            var user = User.Identity?.Name ?? "unknown";

            await queue.EnqueueAsync(new ComputeRequest(jobId, dto.N, user), ct);
            return Accepted(new { jobId });
        }

        [Authorize]
        [HttpGet("{jobId:guid}")]
        public IActionResult Get(Guid jobId)
        {
            if (queue.Results.TryGetValue(jobId, out var r))
                return Ok(new { jobId, r.Status, r.Result, ElapsedMs = r.Elapsed?.TotalMilliseconds });

            return NotFound(new { message = "Job not found" });
        }
    }
}
