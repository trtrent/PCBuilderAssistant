using Azure;
using Azure.AI.OpenAI;
using PCBuildAssistant.Models;
using System.Text.Json;

namespace PCBuildAssistant.Services
{
    public class AzureOpenAIService : IAzureOpenAIService
    {
        private readonly OpenAIClient _openAIClient;
        private readonly string _deploymentName;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIService(IConfiguration configuration, ILogger<AzureOpenAIService> logger)
        {
            _logger = logger;
            
            var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? 
                          configuration["AzureOpenAI:Endpoint"];
            var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? 
                        configuration["AzureOpenAI:ApiKey"];
            _deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? 
                             configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4";

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI endpoint and API key must be configured");
            }

            _openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public async Task<string> GeneratePCBuildRecommendationAsync(PCBuildRequest request)
        {
            try
            {
                var systemPrompt = @"You are an expert PC building assistant with extensive knowledge of current hardware and pricing. 
Generate a detailed PC build recommendation based on the user's preferences. 

Current date: June 2025. Use the latest available hardware information.

Respond with a JSON object containing:
- buildName: A descriptive name for the build
- buildSummary: Brief overview of the build's purpose and strengths
- components: Array of components with name, category, brand, model, estimatedPrice, description, keyFeatures, whyRecommended, compatibilityNotes, and recommendedRetailers
- totalEstimatedCost: Sum of all component prices
- compatibilityWarnings: Any potential compatibility issues
- assemblyTips: Helpful assembly advice
- requiredTools: List of tools needed for assembly
- performanceExpectation: What performance to expect
- recommendedUpgrades: Future upgrade suggestions
- localRetailers: Store recommendations based on location

Include these component categories: CPU, GPU, Motherboard, RAM, Storage (SSD), Power Supply, Case, CPU Cooler, and optionally Monitor, Keyboard, Mouse if requested.

Be specific with model numbers and realistic with pricing. Consider compatibility between components.";

                var userPrompt = $@"Generate a PC build for:
Purpose: {request.Preferences.Purpose}
Budget: {request.Preferences.Budget:C} {request.Preferences.Currency}
Location: {request.Preferences.Location}
Style: {request.Preferences.PreferredStyle}
RGB Lighting: {(request.Preferences.RgbLighting ? "Yes" : "No")}
Connection Type: {request.Preferences.ConnectionType}
Monitor Count: {request.Preferences.MonitorCount}
Monitor Size: {request.Preferences.MonitorSize}
Resolution: {request.Preferences.Resolution}
Preferred Brands: {string.Join(", ", request.Preferences.PreferredBrands)}
Experience Level: {request.Preferences.ExperienceLevel}
Overclocking Interest: {(request.Preferences.OverclockingInterest ? "Yes" : "No")}
Form Factor: {request.Preferences.FormFactor}
Special Requirements: {string.Join(", ", request.Preferences.SpecialRequirements)}
Include Peripherals: {(request.IncludePeripherals ? "Yes" : "No")}
Additional Notes: {request.AdditionalNotes}";

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = _deploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(systemPrompt),
                        new ChatRequestUserMessage(userPrompt)
                    },
                    Temperature = 0.7f,
                    MaxTokens = 4000
                };

                var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                var result = response.Value.Choices[0].Message.Content;

                _logger.LogInformation("Successfully generated PC build recommendation");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PC build recommendation");
                throw new InvalidOperationException("Failed to generate PC build recommendation. Please check your configuration and try again.", ex);
            }
        }

        public async Task<string> GenerateUpgradeRecommendationsAsync(string currentBuild, string improvementGoals)
        {
            try
            {
                var systemPrompt = @"You are an expert PC upgrade advisor. Analyze the current PC build and suggest specific upgrades to meet the user's improvement goals.

Respond with a JSON object containing:
- upgradeRecommendations: Array of upgrade suggestions with component, reason, estimatedCost, performanceGain, difficulty
- priorityOrder: Which upgrades to do first
- budgetOptions: Different upgrade paths for various budgets
- compatibilityNotes: Important compatibility considerations";

                var userPrompt = $@"Current PC Build:
{currentBuild}

Improvement Goals:
{improvementGoals}

Suggest specific upgrades to achieve these goals.";

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = _deploymentName,
                    Messages =
                    {
                        new ChatRequestSystemMessage(systemPrompt),
                        new ChatRequestUserMessage(userPrompt)
                    },
                    Temperature = 0.7f,
                    MaxTokens = 2000
                };

                var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions);
                var result = response.Value.Choices[0].Message.Content;

                _logger.LogInformation("Successfully generated upgrade recommendations");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating upgrade recommendations");
                throw new InvalidOperationException("Failed to generate upgrade recommendations. Please check your configuration and try again.", ex);
            }
        }
    }
}
