using PCBuildAssistant.Models;
using System.Text.Json;
using System.Text;
using iText.Html2pdf;
using iText.Kernel.Pdf;

namespace PCBuildAssistant.Services
{
    public class PCBuildService : IPCBuildService
    {
        private readonly IAzureOpenAIService _azureOpenAIService;
        private readonly ILogger<PCBuildService> _logger;

        public PCBuildService(IAzureOpenAIService azureOpenAIService, ILogger<PCBuildService> logger)
        {
            _azureOpenAIService = azureOpenAIService;
            _logger = logger;
        }

        public async Task<PCBuildResponse> GenerateBuildAsync(PCBuildRequest request)
        {
            try
            {
                var aiResponse = await _azureOpenAIService.GeneratePCBuildRecommendationAsync(request);
                
                _logger.LogInformation("AI response received, attempting to parse JSON");
                
                // Check if response looks like HTML (indicating an error page)
                if (aiResponse.TrimStart().StartsWith("<"))
                {
                    _logger.LogError("AI service returned HTML instead of JSON. Response: {Response}", aiResponse);
                    throw new InvalidOperationException("The AI service returned an error page instead of a JSON response. This usually indicates an authentication or configuration issue.");
                }

                // Log the raw AI response for debugging
                _logger.LogInformation("Raw AI response first 500 chars: {Response}", 
                    aiResponse.Length > 500 ? aiResponse.Substring(0, 500) + "..." : aiResponse);

                // Clean up the AI response - remove markdown code blocks if present
                var cleanedResponse = CleanAIResponse(aiResponse);
                _logger.LogInformation("Cleaned AI response first 200 chars: {Response}", 
                    cleanedResponse.Length > 200 ? cleanedResponse.Substring(0, 200) + "..." : cleanedResponse);
                
                aiResponse = cleanedResponse;
                
                // Parse the AI response JSON with custom options to handle flexible array/string fields
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                // First, try to deserialize as-is
                PCBuildResponse? buildResponse = null;
                try
                {
                    buildResponse = JsonSerializer.Deserialize<PCBuildResponse>(aiResponse, jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize AI response directly, attempting to fix format");
                    // If direct deserialization fails, try to fix common format issues
                    var fixedResponse = FixAIResponseFormat(aiResponse);
                    buildResponse = JsonSerializer.Deserialize<PCBuildResponse>(fixedResponse, jsonOptions);
                }

                if (buildResponse == null)
                {
                    throw new InvalidOperationException("Failed to parse AI response");
                }

                // Set additional metadata
                buildResponse.GeneratedAt = DateTime.UtcNow;
                buildResponse.BuildId = Guid.NewGuid().ToString();
                buildResponse.Currency = request.Preferences.Currency;

                _logger.LogInformation("Successfully generated PC build with ID: {BuildId}", buildResponse.BuildId);
                return buildResponse;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI response JSON");
                throw new InvalidOperationException("The AI service returned an invalid response format.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PC build");
                throw;
            }
        }

        public async Task<byte[]> GenerateBuildPdfAsync(PCBuildResponse buildResponse)
        {
            try
            {
                var html = GenerateHtmlReport(buildResponse);
                
                using var memoryStream = new MemoryStream();
                HtmlConverter.ConvertToPdf(html, memoryStream);
                
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for build {BuildId}", buildResponse.BuildId);
                throw new InvalidOperationException("Failed to generate PDF report.", ex);
            }
        }

        public async Task<string> GenerateBuildTextAsync(PCBuildResponse buildResponse)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"PC Build Report: {buildResponse.BuildName}");
            sb.AppendLine($"Generated: {buildResponse.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine($"Build ID: {buildResponse.BuildId}");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine();
            
            sb.AppendLine("BUILD SUMMARY:");
            sb.AppendLine(buildResponse.BuildSummary);
            sb.AppendLine();
            
            sb.AppendLine("COMPONENTS:");
            sb.AppendLine(new string('-', 40));
            
            foreach (var component in buildResponse.Components)
            {
                sb.AppendLine($"{component.Category}: {component.Name}");
                sb.AppendLine($"  Brand: {component.Brand}");
                sb.AppendLine($"  Model: {component.Model}");
                sb.AppendLine($"  Price: {component.EstimatedPrice:C} {component.Currency}");
                sb.AppendLine($"  Description: {component.Description}");
                sb.AppendLine($"  Why Recommended: {component.WhyRecommended}");
                if (component.KeyFeatures.Any())
                {
                    sb.AppendLine($"  Key Features: {string.Join(", ", component.KeyFeatures)}");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine($"TOTAL ESTIMATED COST: {buildResponse.TotalEstimatedCost:C} {buildResponse.Currency}");
            sb.AppendLine();
            
            if (buildResponse.CompatibilityWarnings.Any())
            {
                sb.AppendLine("COMPATIBILITY WARNINGS:");
                foreach (var warning in buildResponse.CompatibilityWarnings)
                {
                    sb.AppendLine($"  • {warning}");
                }
                sb.AppendLine();
            }
            
            if (buildResponse.RequiredTools.Any())
            {
                sb.AppendLine("REQUIRED TOOLS:");
                foreach (var tool in buildResponse.RequiredTools)
                {
                    sb.AppendLine($"  • {tool}");
                }
                sb.AppendLine();
            }
            
            if (buildResponse.AssemblyTips.Any())
            {
                sb.AppendLine("ASSEMBLY TIPS:");
                foreach (var tip in buildResponse.AssemblyTips)
                {
                    sb.AppendLine($"  • {tip}");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("PERFORMANCE EXPECTATION:");
            sb.AppendLine(buildResponse.PerformanceExpectation);
            sb.AppendLine();
            
            if (buildResponse.RecommendedUpgrades.Any())
            {
                sb.AppendLine("FUTURE UPGRADE RECOMMENDATIONS:");
                foreach (var upgrade in buildResponse.RecommendedUpgrades)
                {
                    sb.AppendLine($"  • {upgrade}");
                }
                sb.AppendLine();
            }
            
            if (buildResponse.LocalRetailers.Any())
            {
                sb.AppendLine("RECOMMENDED RETAILERS:");
                foreach (var retailer in buildResponse.LocalRetailers)
                {
                    sb.AppendLine($"  • {retailer}");
                }
            }
            
            return sb.ToString();
        }

        private string GenerateHtmlReport(PCBuildResponse buildResponse)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>PC Build Report - {buildResponse.BuildName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ border-bottom: 2px solid #333; margin-bottom: 20px; }}
        .component {{ margin-bottom: 20px; padding: 10px; border: 1px solid #ddd; }}
        .component-title {{ font-weight: bold; color: #333; }}
        .price {{ color: #007bff; font-weight: bold; }}
        .total {{ font-size: 18px; font-weight: bold; color: #28a745; }}
        .warning {{ color: #dc3545; }}
        .tip {{ color: #6c757d; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>PC Build Report: {buildResponse.BuildName}</h1>
        <p>Generated: {buildResponse.GeneratedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
        <p>Build ID: {buildResponse.BuildId}</p>
    </div>
    
    <h2>Build Summary</h2>
    <p>{buildResponse.BuildSummary}</p>
    
    <h2>Components</h2>";

            foreach (var component in buildResponse.Components)
            {
                html += $@"
    <div class='component'>
        <div class='component-title'>{component.Category}: {component.Name}</div>
        <p><strong>Brand:</strong> {component.Brand}</p>
        <p><strong>Model:</strong> {component.Model}</p>
        <p><strong>Price:</strong> <span class='price'>{component.EstimatedPrice:C} {component.Currency}</span></p>
        <p><strong>Description:</strong> {component.Description}</p>
        <p><strong>Why Recommended:</strong> {component.WhyRecommended}</p>";

                if (component.KeyFeatures.Any())
                {
                    html += $"<p><strong>Key Features:</strong> {string.Join(", ", component.KeyFeatures)}</p>";
                }

                html += "</div>";
            }

            html += $@"
    <div class='total'>
        <h2>Total Estimated Cost: {buildResponse.TotalEstimatedCost:C} {buildResponse.Currency}</h2>
    </div>";

            if (buildResponse.CompatibilityWarnings.Any())
            {
                html += "<h2>Compatibility Warnings</h2><ul>";
                foreach (var warning in buildResponse.CompatibilityWarnings)
                {
                    html += $"<li class='warning'>{warning}</li>";
                }
                html += "</ul>";
            }

            if (buildResponse.RequiredTools.Any())
            {
                html += "<h2>Required Tools</h2><ul>";
                foreach (var tool in buildResponse.RequiredTools)
                {
                    html += $"<li>{tool}</li>";
                }
                html += "</ul>";
            }

            if (buildResponse.AssemblyTips.Any())
            {
                html += "<h2>Assembly Tips</h2><ul>";
                foreach (var tip in buildResponse.AssemblyTips)
                {
                    html += $"<li class='tip'>{tip}</li>";
                }
                html += "</ul>";
            }

            html += $@"
    <h2>Performance Expectation</h2>
    <p>{buildResponse.PerformanceExpectation}</p>";

            if (buildResponse.RecommendedUpgrades.Any())
            {
                html += "<h2>Future Upgrade Recommendations</h2><ul>";
                foreach (var upgrade in buildResponse.RecommendedUpgrades)
                {
                    html += $"<li>{upgrade}</li>";
                }
                html += "</ul>";
            }

            if (buildResponse.LocalRetailers.Any())
            {
                html += "<h2>Recommended Retailers</h2><ul>";
                foreach (var retailer in buildResponse.LocalRetailers)
                {
                    html += $"<li>{retailer}</li>";
                }
                html += "</ul>";
            }

            html += @"
</body>
</html>";

            return html;
        }

        private string FixAIResponseFormat(string aiResponse)
        {
            try
            {
                // Parse as JsonDocument to examine and fix the structure
                using var doc = JsonDocument.Parse(aiResponse);
                var root = doc.RootElement;
                
                // Create a new object with fixed array fields
                var fixedObject = new Dictionary<string, object>();
                
                foreach (var property in root.EnumerateObject())
                {
                    var value = property.Value;
                    var name = property.Name;
                    
                    if (name.Equals("components", StringComparison.OrdinalIgnoreCase) && value.ValueKind == JsonValueKind.Array)
                    {
                        var components = new List<object>();
                        foreach (var component in value.EnumerateArray())
                        {
                            var componentObj = new Dictionary<string, object>();
                            foreach (var compProp in component.EnumerateObject())
                            {
                                var compValue = compProp.Value;
                                var compName = compProp.Name;
                                
                                // Fix array fields that might be strings
                                if ((compName.Equals("keyFeatures", StringComparison.OrdinalIgnoreCase) ||
                                     compName.Equals("compatibilityNotes", StringComparison.OrdinalIgnoreCase) ||
                                     compName.Equals("recommendedRetailers", StringComparison.OrdinalIgnoreCase)) &&
                                    compValue.ValueKind == JsonValueKind.String)
                                {
                                    var stringValue = compValue.GetString() ?? "";
                                    // Convert comma-separated string to array
                                    componentObj[compName] = stringValue.Split(',')
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .ToArray();
                                }
                                else
                                {
                                    componentObj[compName] = GetJsonValue(compValue);
                                }
                            }
                            components.Add(componentObj);
                        }
                        fixedObject[name] = components;
                    }
                    else if ((name.Equals("compatibilityWarnings", StringComparison.OrdinalIgnoreCase) ||
                              name.Equals("assemblyTips", StringComparison.OrdinalIgnoreCase) ||
                              name.Equals("requiredTools", StringComparison.OrdinalIgnoreCase) ||
                              name.Equals("recommendedUpgrades", StringComparison.OrdinalIgnoreCase) ||
                              name.Equals("localRetailers", StringComparison.OrdinalIgnoreCase)) &&
                             value.ValueKind == JsonValueKind.String)
                    {
                        var stringValue = value.GetString() ?? "";
                        // Convert comma-separated string to array
                        fixedObject[name] = stringValue.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToArray();
                    }
                    else
                    {
                        fixedObject[name] = GetJsonValue(value);
                    }
                }
                
                return JsonSerializer.Serialize(fixedObject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fix AI response format");
                throw new InvalidOperationException("The AI service returned an invalid response format that could not be fixed.", ex);
            }
        }
        
        private object GetJsonValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? "",
                JsonValueKind.Number => element.TryGetDecimal(out var dec) ? dec : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(GetJsonValue).ToArray(),
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => GetJsonValue(p.Value)),
                _ => element.GetRawText()
            };
        }

        private string CleanAIResponse(string aiResponse)
        {
            if (string.IsNullOrEmpty(aiResponse))
                return aiResponse;

            // Trim whitespace
            aiResponse = aiResponse.Trim();

            // Remove markdown code blocks if present
            if (aiResponse.StartsWith("```json"))
            {
                // Remove opening ```json
                aiResponse = aiResponse.Substring(7).Trim();
                
                // Remove closing ```
                if (aiResponse.EndsWith("```"))
                {
                    aiResponse = aiResponse.Substring(0, aiResponse.Length - 3).Trim();
                }
            }
            else if (aiResponse.StartsWith("```"))
            {
                // Remove opening ```
                aiResponse = aiResponse.Substring(3).Trim();
                
                // Remove closing ```
                if (aiResponse.EndsWith("```"))
                {
                    aiResponse = aiResponse.Substring(0, aiResponse.Length - 3).Trim();
                }
            }
            else if (aiResponse.StartsWith("`") && aiResponse.EndsWith("`"))
            {
                // Remove single backticks
                aiResponse = aiResponse.Substring(1, aiResponse.Length - 2).Trim();
            }

            return aiResponse;
        }
    }
}
