namespace KıbrısApp3.DTO
{
    public class SendLocationDto
    {
        public string ReceiverId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Message { get; set; } // İsteğe bağlı açıklama
    }
}
