namespace JobPortalManagement.Models
{
    public class RegistrationData
    {
        public int Id { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        // No DynamicData; columns like FirstName, Email will be added to the table dynamically
    }
}
