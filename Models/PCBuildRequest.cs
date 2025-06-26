namespace PCBuildAssistant.Models
{
    public class PCBuildRequest
    {
        public UserPreferences Preferences { get; set; } = new();
        public bool IncludePeripherals { get; set; } = true;
        public bool IncludeAssemblyGuide { get; set; } = true;
        public string AdditionalNotes { get; set; } = string.Empty;
    }
}
