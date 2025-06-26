using Microsoft.AspNetCore.Mvc;
using PCBuildAssistant.Models;
using PCBuildAssistant.Services;

namespace PCBuildAssistant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PCBuildController : ControllerBase
    {
        private readonly IPCBuildService _pcBuildService;
        private readonly ILogger<PCBuildController> _logger;

        public PCBuildController(IPCBuildService pcBuildService, ILogger<PCBuildController> logger)
        {
            _pcBuildService = pcBuildService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<PCBuildResponse>> GenerateBuild([FromBody] PCBuildRequest request)
        {
            try
            {
                if (request?.Preferences == null)
                {
                    return BadRequest("Invalid request. User preferences are required.");
                }

                if (request.Preferences.Budget <= 0)
                {
                    return BadRequest("Budget must be greater than zero.");
                }

                var result = await _pcBuildService.GenerateBuildAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while generating PC build");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while generating PC build");
                return StatusCode(500, "An unexpected error occurred while generating your PC build. Please try again later.");
            }
        }

        [HttpPost("download/pdf")]
        public async Task<IActionResult> DownloadBuildPdf([FromBody] PCBuildResponse buildResponse)
        {
            try
            {
                if (buildResponse == null)
                {
                    return BadRequest("Build response is required.");
                }

                var pdfBytes = await _pcBuildService.GenerateBuildPdfAsync(buildResponse);
                var fileName = $"PC_Build_{buildResponse.BuildName}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for build {BuildId}", buildResponse?.BuildId);
                return StatusCode(500, "Failed to generate PDF. Please try again later.");
            }
        }

        [HttpPost("download/text")]
        public async Task<IActionResult> DownloadBuildText([FromBody] PCBuildResponse buildResponse)
        {
            try
            {
                if (buildResponse == null)
                {
                    return BadRequest("Build response is required.");
                }

                var textContent = await _pcBuildService.GenerateBuildTextAsync(buildResponse);
                var fileName = $"PC_Build_{buildResponse.BuildName}_{DateTime.UtcNow:yyyyMMdd}.txt";
                
                var bytes = System.Text.Encoding.UTF8.GetBytes(textContent);
                return File(bytes, "text/plain", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text file for build {BuildId}", buildResponse?.BuildId);
                return StatusCode(500, "Failed to generate text file. Please try again later.");
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}
