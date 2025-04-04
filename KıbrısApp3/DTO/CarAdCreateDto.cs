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
        public int CategoryId { get; set; }

        [Required]
        public string Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        public List<string> Base64Images { get; set; } = new();


        // Araç özel alanlar
        [Required]
        public string Brand { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string FuelType { get; set; }

        [Required]
        public string Transmission { get; set; }

        public int EngineSize { get; set; }
    }
}
