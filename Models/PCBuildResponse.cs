namespace PCBuildAssistant.Models
{
    public class PCBuildResponse
    {
        public string BuildName { get; set; } = string.Empty;
        public string BuildSummary { get; set; } = string.Empty;
        public List<ComponentInfo> Components { get; set; } = new();
        public decimal TotalEstimatedCost { get; set; }
        public string Currency { get; set; } = "USD";
        public List<string> CompatibilityWarnings { get; set; } = new();
        public List<string> AssemblyTips { get; set; } = new();
        public List<string> RequiredTools { get; set; } = new();
        public string PerformanceExpectation { get; set; } = string.Empty;
        public List<string> RecommendedUpgrades { get; set; } = new();
        public List<string> LocalRetailers { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string BuildId { get; set; } = Guid.NewGuid().ToString();
    }
}
