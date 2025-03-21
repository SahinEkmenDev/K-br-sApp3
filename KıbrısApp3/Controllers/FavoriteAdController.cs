using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KıbrısApp3.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize]  // Kullanıcı girişi gereklidir
    public class FavoriteAdController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoriteAdController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📌 Favori ilan ekleme
        [HttpPost("add/{adId}")]
        public async Task<IActionResult> AddToFavorites(int adId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            var alreadyExists = await _context.FavoriteAds.AnyAsync(f => f.UserId == userId && f.AdListingId == adId);
            if (alreadyExists)
                return BadRequest(new { message = "Bu ilan zaten favorilerinizde!" });

            var favorite = new FavoriteAd { UserId = userId, AdListingId = adId };
            _context.FavoriteAds.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan favorilere eklendi!" });
        }

        // 📌 Favori ilanları listeleme
        [HttpGet]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            var favorites = await _context.FavoriteAds
                .Where(f => f.UserId == userId)
                .Include(f => f.AdListing)
                .ThenInclude(a => a.User)  // Satıcı bilgisi de getirilsin
                .Select(f => new
                {
                    f.AdListing.Id,
                    f.AdListing.Title,
                    f.AdListing.Description,
                    f.AdListing.Price,
                    f.AdListing.ImageUrl,
                    f.AdListing.CategoryId,
                    CategoryName = f.AdListing.Category.Name,
                    SellerName = f.AdListing.User.FullName
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // 📌 Favori ilan kaldırma
        [HttpDelete("remove/{adId}")]
        public async Task<IActionResult> RemoveFromFavorites(int adId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            var favorite = await _context.FavoriteAds
                .FirstOrDefaultAsync(f => f.UserId == userId && f.AdListingId == adId);

            if (favorite == null)
                return NotFound(new { message = "Bu ilan favorilerde bulunamadı!" });

            _context.FavoriteAds.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan favorilerden kaldırıldı!" });
        }
    }
}
