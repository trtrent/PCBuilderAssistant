namespace PCBuildAssistant.Models
{
    public class ComponentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // CPU, GPU, RAM, etc.
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public decimal EstimatedPrice { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = string.Empty;
        public List<string> KeyFeatures { get; set; } = new();
        public string WhyRecommended { get; set; } = string.Empty;
        public List<string> CompatibilityNotes { get; set; } = new();
        public string Availability { get; set; } = string.Empty;
        public List<string> RecommendedRetailers { get; set; } = new();
    }
}
