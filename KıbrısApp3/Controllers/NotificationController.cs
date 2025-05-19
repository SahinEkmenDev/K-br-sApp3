using KıbrısApp3.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KıbrısApp3.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly IConfiguration _config;

        public NotificationController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] FcmNotificationDto dto)
        {
            var serverKey = _config["Firebase:ServerKey"]; // appsettings.json’dan gelecek
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + serverKey);

            var data = new
            {
                to = dto.FcmToken,
                notification = new
                {
                    title = dto.Title,
                    body = dto.Body
                }
            };

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", content);

            if (response.IsSuccessStatusCode)
                return Ok(new { message = "Bildirim gönderildi!" });

            var error = await response.Content.ReadAsStringAsync();
            return StatusCode(500, new { message = "Gönderim hatası!", error });
        }
    }
}
