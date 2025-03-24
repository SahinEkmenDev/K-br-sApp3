namespace KıbrısApp3.Models
{
    public class AdImage
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public int AdListingId { get; set; }
        public AdListing AdListing { get; set; }
    }
}
