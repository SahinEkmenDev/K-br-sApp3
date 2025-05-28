using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
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

        [HttpPost("send-image")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendImageMessage([FromForm] SendImageMessageDto dto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId))
                return Unauthorized();

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            try
            {
                // appsettings.json'dan Cloudinary bilgilerini oku
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var account = new Account(
                    config["Cloudinary:CloudName"],
                    config["Cloudinary:ApiKey"],
                    config["Cloudinary:ApiSecret"]
                );

                var cloudinary = new Cloudinary(account);

                using var memoryStream = new MemoryStream();
                await dto.File.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(dto.File.FileName, memoryStream),
                    Folder = "message-images"
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    return StatusCode(500, new { message = "Cloudinary yükleme başarısız." });

                var imageUrl = uploadResult.SecureUrl.ToString();

                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = dto.ReceiverId,
                    ImageUrl = imageUrl,
                    Timestamp = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Fotoğraf gönderildi", imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Yükleme sırasında hata oluştu.", error = ex.Message });
            }
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
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    SenderUserName = m.Sender.UserName,
                    m.ReceiverId,
                    ReceiverUserName = m.Receiver.UserName,
                    m.Content,
                    m.ImageUrl,
                    m.Latitude,
                    m.Longitude,
                    m.Timestamp
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("conversation-list")]
        [Authorize]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conversations = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.Timestamp)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault().Content,
                    LastMessageTime = g.OrderByDescending(m => m.Timestamp).FirstOrDefault().Timestamp,
                    User = _context.Users
                            .Where(u => u.Id == g.Key)
                            .Select(u => new {
                                u.Id,
                                FullName = u.FullName,
                                ProfilePictureUrl = u.ProfileImageUrl


                            })
                            .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(conversations);
        }
        [HttpGet("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetMessagesWithUser(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == userId) ||
                    (m.SenderId == userId && m.ReceiverId == currentUserId)
                )
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    senderUserName = m.Sender.UserName,
                    m.ReceiverId,
                    receiverUserName = m.Receiver.UserName,
                    m.Content,
                    m.ImageUrl,
                    m.Latitude,
                    m.Longitude,
                    m.Timestamp
                })
                .ToListAsync();

            return Ok(messages);
        }





        [HttpPost("mark-as-read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return NotFound();

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mesaj okundu olarak işaretlendi." });
        }
        [HttpDelete("delete-conversation/{otherUserId}")]
        public async Task<IActionResult> DeleteConversation(string otherUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == userId))
                .ToListAsync();

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Konuşma silindi." });
        }





    }
}
