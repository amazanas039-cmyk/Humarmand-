namespace SkillBridge.Helpers
{
    public static class SessionKeys
    {
        public const string UserId       = "UserId";       // int
        public const string UserRole     = "UserRole";     // "Customer"|"Labourer"|"Admin"
        public const string UserFullName = "UserFullName"; // string
        public const string LabourerId   = "LabourerId";   // int — set if Role == "Labourer"
        public const string CustomerId   = "CustomerId";   // int — set if Role == "Customer"
        public const string AdminId      = "AdminId";      // int — set if Role == "Admin"
    }
}
