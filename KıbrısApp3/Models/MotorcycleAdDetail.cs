namespace KıbrısApp3.Models
{
    public class MotorcycleAdDetail
    {
        public int Id { get; set; }
        public int AdListingId { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Kilometre { get; set; }
        public int HorsePower { get; set; }
        public int EngineSize { get; set; }

        public AdListing AdListing { get; set; }
    }

}
