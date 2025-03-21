using KıbrısApp3.Data;
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
        public async Task<IActionResult> SendMessage([FromBody] Message model)
        {
            var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            if (string.IsNullOrEmpty(model.ReceiverId) || string.IsNullOrEmpty(model.Content))
                return BadRequest(new { message = "Alıcı ID ve mesaj içeriği gereklidir!" });

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = model.ReceiverId,
                Content = model.Content,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mesaj gönderildi!" });
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
