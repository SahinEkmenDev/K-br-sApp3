namespace KıbrısApp3.DTO
{
    public class FcmNotificationDto
    {
        public string Title { get; set; }        // Bildirim başlığı
        public string Body { get; set; }         // Bildirim mesajı
        public string FcmToken { get; set; }     // Kullanıcının cihaz token'ı
    }
}
