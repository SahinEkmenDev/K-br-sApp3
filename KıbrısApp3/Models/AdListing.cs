using System.Text.Json.Serialization;

namespace KıbrısApp3.Models
{
    public class AdListing
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }

        [JsonIgnore]  // 📌 Swagger'da Category objesini gösterme
        public Category Category { get; set; }

        public string UserId { get; set; }

        [JsonIgnore]  // 📌 Swagger'da User objesini gösterme
        public ApplicationUser User { get; set; }

        public string Status { get; set; } = "Beklemede";
    }
}
