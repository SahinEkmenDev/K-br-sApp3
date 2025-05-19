using System.ComponentModel.DataAnnotations;

public class AdListingCreateDto
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    [RegularExpression("^(TRY|GBP)$", ErrorMessage = "Para birimi sadece 'TRY' veya 'GBP' olabilir.")]
    public string Currency { get; set; } = "TRY";


    public string? ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string? Status { get; set; }

    [Required]
    public string Address { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public List<string> Base64Images { get; set; } = new(); // boş olmasın
}
