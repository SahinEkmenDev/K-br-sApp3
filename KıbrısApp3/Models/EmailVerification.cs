namespace KıbrısApp3.Models
{
    public class EmailVerification
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime ExpireAt { get; set; }
    }

}
