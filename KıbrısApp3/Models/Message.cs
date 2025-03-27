namespace KıbrısApp3.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }
        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;


        // Yeni eklenen alanlar:
        public string? ImageUrl { get; set; } // Resim dosyası URL’si
        public double? Latitude { get; set; } // Konum - enlem
        public double? Longitude { get; set; } // Konum - boylam
    }

}

