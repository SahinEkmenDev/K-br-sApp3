using KıbrısApp3.Models;
using System.Text.Json.Serialization;

public class AdListing
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } // Opsiyonel olarak bırakabilirsin (tek görsel için)

    public string Address { get; set; }
    public int CategoryId { get; set; }

    [JsonIgnore]
    public Category Category { get; set; }

    public string UserId { get; set; }

    [JsonIgnore]
    public ApplicationUser User { get; set; }

    public string Status { get; set; } = "Beklemede";

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    // ✅ Çoklu görsel desteği
    public ICollection<AdImage> Images { get; set; }
}
