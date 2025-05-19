namespace KıbrısApp3.Models
{
    public class CarAdDetail
    {
        public int Id { get; set; }
        public int AdListingId { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Kilometre { get; set; }
        public int HorsePower { get; set; }      // Motor gücü (HP)
        public int EngineSize { get; set; }      // Motor hacmi (cc)
        public string BodyType { get; set; }     // Kasa tipi
        public string Transmission { get; set; } // Vites
        public string FuelType { get; set; }     // Yakıt tipi

        public AdListing AdListing { get; set; }
    }

}
