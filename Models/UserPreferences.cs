namespace PCBuildAssistant.Models
{
    public class UserPreferences
    {
        public string Purpose { get; set; } = string.Empty; // Gaming, Coding, Video Editing, etc.
        public decimal Budget { get; set; }
        public string Currency { get; set; } = "USD";
        public string Location { get; set; } = string.Empty; // Country/Region
        public string PreferredStyle { get; set; } = string.Empty; // Minimalist, RGB, Professional, etc.
        public bool RgbLighting { get; set; }
        public string ConnectionType { get; set; } = string.Empty; // Wired, Wireless, Mixed
        public int MonitorCount { get; set; } = 1;
        public string MonitorSize { get; set; } = string.Empty; // 24", 27", 32", etc.
        public string Resolution { get; set; } = string.Empty; // 1080p, 1440p, 4K
        public List<string> PreferredBrands { get; set; } = new();
        public string ExperienceLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced
        public bool OverclockingInterest { get; set; }
        public string FormFactor { get; set; } = string.Empty; // ATX, Micro-ATX, Mini-ITX
        public List<string> SpecialRequirements { get; set; } = new(); // Quiet operation, Compact size, etc.
    }
}
