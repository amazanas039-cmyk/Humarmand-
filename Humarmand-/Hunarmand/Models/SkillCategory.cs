namespace Hunarmand.Models
{
    public class SkillCategory
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string IconClass { get; set; } = ""; // Helper for FontAwesome icon on landing
    }
}
