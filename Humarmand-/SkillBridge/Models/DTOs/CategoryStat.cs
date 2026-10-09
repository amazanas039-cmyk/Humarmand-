namespace SkillBridge.Models.DTOs
{
    public class CategoryStat
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public string Color { get; set; } = ""; // Palette cycles in service
    }
}
