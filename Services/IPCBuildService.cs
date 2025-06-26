using PCBuildAssistant.Models;

namespace PCBuildAssistant.Services
{
    public interface IPCBuildService
    {
        Task<PCBuildResponse> GenerateBuildAsync(PCBuildRequest request);
        Task<byte[]> GenerateBuildPdfAsync(PCBuildResponse buildResponse);
        Task<string> GenerateBuildTextAsync(PCBuildResponse buildResponse);
    }
}
