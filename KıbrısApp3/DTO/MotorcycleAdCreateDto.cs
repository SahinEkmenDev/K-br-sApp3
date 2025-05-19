namespace KıbrısApp3.DTO
{
    public class MotorcycleAdCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int CategoryId { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public List<string> Base64Images { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Kilometre { get; set; }
        public int HorsePower { get; set; }
        public int EngineSize { get; set; }
    }

}
