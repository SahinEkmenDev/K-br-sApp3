namespace KıbrısApp3.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } // Gönderen kullanıcı
        public string ReceiverId { get; set; } // Alıcı kullanıcı
        public string Content { get; set; } // Mesaj içeriği
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Mesaj zamanı
    }
}

