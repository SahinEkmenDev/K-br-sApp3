using KıbrısApp3.Data;
using KıbrısApp3.DTO;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KıbrısApp3.Controllers
{
    [Route("api/messages")]
    [ApiController]
    [Authorize] // Kullanıcı girişi gereklidir
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto dto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId)) return Unauthorized();

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mesaj gönderildi." });
        }


        [HttpPost("upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(new { imageUrl });
        }
        [HttpPost("send-location")]
        [Authorize]
        public async Task<IActionResult> SendLocation([FromBody] SendLocationDto model)
        {
            var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            if (string.IsNullOrEmpty(model.ReceiverId))
                return BadRequest(new { message = "Alıcı bilgisi gereklidir!" });

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = model.ReceiverId,
                Content = model.Message, // Açıklama (opsiyonel)
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Konum başarıyla gönderildi." });
        }





        [HttpGet("conversations")]
        [Authorize]
        public async Task<IActionResult> GetUserConversations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            var messages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Content,
                    m.Timestamp
                })
                .ToListAsync();

            return Ok(messages);
        }


    }
}
