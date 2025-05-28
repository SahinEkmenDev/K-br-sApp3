using Microsoft.AspNetCore.Http;

namespace KıbrısApp3.DTO
{

    public class SendImageMessageDto
    {
        public IFormFile File { get; set; }
        public string ReceiverId { get; set; }
    }

}
