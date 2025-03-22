using KıbrısApp3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KıbrısApp3.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📌 Giriş yapan kullanıcının profilini getir
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            var ads = await _context.AdListings
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Price,
                    a.ImageUrl,
                    a.Status,
                    CategoryName = a.Category.Name
                })
                .ToListAsync();

            return Ok(new
            {
                User = user,
                MyAds = ads
            });
        }
    }
}
