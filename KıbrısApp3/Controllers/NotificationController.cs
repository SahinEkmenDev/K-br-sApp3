using KıbrısApp3.Data;
using KıbrısApp3.DTO;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace KıbrısApp3.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register-token")]
        [Authorize]
        public async Task<IActionResult> RegisterExpoToken([FromBody] string expoPushToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existing = await _context.UserExpoTokens
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (existing != null)
            {
                existing.ExpoPushToken = expoPushToken;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var token = new UserExpoToken
                {
                    UserId = userId,
                    ExpoPushToken = expoPushToken
                };
                await _context.UserExpoTokens.AddAsync(token);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Expo token kaydedildi." });
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationDto dto)
        {
            var userToken = await _context.UserExpoTokens
                .FirstOrDefaultAsync(x => x.UserId == dto.UserId);

            if (userToken == null)
                return NotFound(new { message = "Kullanıcıya ait token bulunamadı." });

            var isSuccess = await SendExpoNotification(userToken.ExpoPushToken, dto.Title, dto.Body);
            return Ok(new { success = isSuccess });
        }

        private async Task<bool> SendExpoNotification(string expoPushToken, string title, string body)
        {
            using var client = new HttpClient();
            var payload = new
            {
                to = expoPushToken,
                title = title,
                body = body
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://exp.host/--/api/v2/push/send", content);
            return response.IsSuccessStatusCode;
        }
    }


}

