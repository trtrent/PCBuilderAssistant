using PCBuildAssistant.Models;

namespace PCBuildAssistant.Services
{
    public interface IAzureOpenAIService
    {
        Task<string> GeneratePCBuildRecommendationAsync(PCBuildRequest request);
        Task<string> GenerateUpgradeRecommendationsAsync(string currentBuild, string improvementGoals);
    }
}
