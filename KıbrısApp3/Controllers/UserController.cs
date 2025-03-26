using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;


namespace KıbrısApp3.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly UserManager<ApplicationUser> _userManager;


        public UserController(ApplicationDbContext context, IConfiguration config, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        [HttpPost("upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture([FromBody] string base64Image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            string actualBase64 = base64Image;
            if (base64Image.Contains(","))
            {
                var parts = base64Image.Split(',');
                if (parts.Length == 2)
                    actualBase64 = parts[1];
            }

            try
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription($"profile_{userId}", new MemoryStream(Convert.FromBase64String(actualBase64))),
                    Folder = "profile-pictures"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    user.ProfileImageUrl = uploadResult.SecureUrl.ToString();
                    await _userManager.UpdateAsync(user);

                    return Ok(new { imageUrl = user.ProfileImageUrl });
                }

                return StatusCode(500, new { message = "Cloudinary yükleme başarısız!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Profil fotoğrafı yüklenemedi!", error = ex.Message });
            }
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
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] ApplicationUser model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profil güncellendi." });
        }
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Hesabınız silindi." });
        }
        [HttpGet("me/favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _context.FavoriteAds
                .Where(f => f.UserId == userId)
                .Include(f => f.AdListing)
                .ThenInclude(a => a.Category)
                .Select(f => new
                {
                    f.AdListing.Id,
                    f.AdListing.Title,
                    f.AdListing.Price,
                    f.AdListing.ImageUrl,
                    f.AdListing.Status,
                    CategoryName = f.AdListing.Category.Name
                })
                .ToListAsync();

            return Ok(favorites);
        }
        [HttpGet("me/messages")]
        public async Task<IActionResult> GetMyMessages()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var messages = await _context.Messages
                .Where(m => m.ReceiverId == userId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.Timestamp,
                    Sender = new { m.Sender.Id, m.Sender.FullName, m.Sender.Email }
                })
                .ToListAsync();

            return Ok(messages);
        }
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateAdStatus(int id, [FromQuery] string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ad = await _context.AdListings.FindAsync(id);

            if (ad == null) return NotFound();
            if (ad.UserId != userId) return Unauthorized();

            ad.Status = status; // "Satıldı", "Yayında", "Beklemede"
            await _context.SaveChangesAsync();

            return Ok(new { message = $"İlan durumu '{status}' olarak güncellendi." });
        }





    }
}
