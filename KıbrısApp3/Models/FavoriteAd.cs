namespace KıbrısApp3.Models
{
    public class FavoriteAd
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // Favori ekleyen kullanıcı
        public ApplicationUser User { get; set; }

        public int AdListingId { get; set; }  // Favoriye eklenen ilan
        public AdListing AdListing { get; set; }
    }
}
