namespace KıbrısApp3.Models
{
    public class UserExpoToken
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // AspNetUsers tablosuyla ilişki
        public string ExpoPushToken { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser User { get; set; }
    }
}
