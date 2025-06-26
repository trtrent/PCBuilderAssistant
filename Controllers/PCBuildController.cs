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
                _logger.LogInformation("Received PC build generation request");
                
                if (request?.Preferences == null)
                {
                    _logger.LogWarning("Invalid request: User preferences are null");
                    return BadRequest(new { error = "Invalid request. User preferences are required." });
                }

                if (request.Preferences.Budget <= 0)
                {
                    _logger.LogWarning("Invalid request: Budget is {Budget}", request.Preferences.Budget);
                    return BadRequest(new { error = "Budget must be greater than zero." });
                }

                _logger.LogInformation("Generating PC build for purpose: {Purpose}, budget: {Budget}", 
                    request.Preferences.Purpose, request.Preferences.Budget);
                
                var result = await _pcBuildService.GenerateBuildAsync(request);
                
                _logger.LogInformation("Successfully generated PC build with ID: {BuildId}", result.BuildId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while generating PC build");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while generating PC build");
                return StatusCode(500, new { error = "An unexpected error occurred while generating your PC build. Please check the configuration and try again." });
            }
        }

        [HttpGet("config-check")]
        public IActionResult ConfigCheck()
        {
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
            var hasApiKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"));
            var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
            
            return Ok(new { 
                hasEndpoint = !string.IsNullOrEmpty(endpoint),
                endpointDomain = endpoint != null ? new Uri(endpoint).Host : "not set",
                hasApiKey = hasApiKey,
                deployment = deployment ?? "gpt-4 (default)"
            });
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
