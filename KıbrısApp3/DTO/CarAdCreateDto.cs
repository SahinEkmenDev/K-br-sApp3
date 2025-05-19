using System.ComponentModel.DataAnnotations;

namespace KıbrısApp3.DTO
{
    public class CarAdCreateDto
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


        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        public List<string> Base64Images { get; set; } = new();


        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Kilometre { get; set; }
        public int HorsePower { get; set; }
        public int EngineSize { get; set; }
        public string BodyType { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
    }
}
