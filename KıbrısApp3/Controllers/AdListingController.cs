// AdListingController.cs
using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace KıbrısApp3.Controllers
{
    [Route("api/ad-listings")]
    [ApiController]
    public class AdListingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdListingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📌 Gelişmiş İlan Arama & Filtreleme
        [HttpGet("search")]
        public async Task<IActionResult> SearchAds(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? sortBy)
        {
            var query = _context.AdListings.Include(a => a.Category).AsQueryable();

            // 📌 Başlık veya açıklamada anahtar kelime arama
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Description.Contains(keyword));
            }

            // 📌 Kategori filtresi
            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            // 📌 Fiyat aralığı filtresi
            if (minPrice.HasValue)
            {
                query = query.Where(a => a.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(a => a.Price <= maxPrice.Value);
            }

            // 📌 Sıralama (fiyat veya en yeni ilanlar)
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(a => a.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(a => a.Price);
                    break;
                case "newest":
                    query = query.OrderByDescending(a => a.Id);
                    break;
            }

            var ads = await query.ToListAsync();
            return Ok(ads);
        }
        [HttpGet("user-ads")]
        [Authorize]
        public async Task<IActionResult> GetUserAds()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("⚠️ Kullanıcı ID bulunamadı!");
                    return Unauthorized(new { message = "Kullanıcı bulunamadı." });
                }

                var ads = await _context.AdListings
                                         .Where(a => a.UserId == userId)
                                         .ToListAsync();

                if (ads == null || ads.Count == 0)
                {
                    Console.WriteLine("⚠️ Kullanıcının hiç ilanı yok!");
                }

                return Ok(ads);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Sunucu Hatası: {ex.Message}");
                return StatusCode(500, new { message = "Sunucu hatası!", error = ex.Message });
            }
        }



        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetAdsByCategory(int categoryId)
        {
            var ads = await _context.AdListings
                                     .Include(a => a.Category)
                                     .Include(a => a.User) // 📌 Kullanıcı bilgilerini de ekledik
                                     .Where(a => a.CategoryId == categoryId)
                                     .Select(a => new
                                     {
                                         a.Id,
                                         a.Title,
                                         a.Description,
                                         a.Price,
                                         a.ImageUrl,
                                         a.CategoryId,
                                         CategoryName = a.Category.Name,
                                         a.UserId,
                                         SellerName = a.User.FullName, // 📌 Kullanıcının adını ekledik
                                         a.Status
                                     })
                                     .ToListAsync();

            return Ok(ads);
        }


        // 📌 İlan ekleme (sadece giriş yapmış kullanıcılar)
        [HttpPost]
        [Authorize]  // Kullanıcı giriş yapmış olmalı
        public async Task<IActionResult> AddAd([FromBody] AdListing model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Giriş yapmalısınız!" });

            var ad = new AdListing
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                UserId = userId,  // Oturum açan kullanıcı ID'si otomatik atanıyor
                Status = "Yayında"  // Varsayılan olarak "Yayında" olacak
            };

            _context.AdListings.Add(ad);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan başarıyla eklendi!", ad });
        }


        private async Task<bool> IsOwner(int adId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("⚠️ Kullanıcı ID bulunamadı!");
                return false;
            }

            var ad = await _context.AdListings.FindAsync(adId);
            if (ad == null)
            {
                Console.WriteLine($"⚠️ İlan bulunamadı! ID: {adId}");
                return false;
            }

            return ad.UserId == userId;
        }


        // 📌 İlan güncelleme (sadece ilan sahibi yapabilir)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAd(int id, [FromBody] AdListing model)
        {
            if (!await IsOwner(id))
                return Forbid(); // Kullanıcının ilanı değilse işlem yapamaz

            var ad = await _context.AdListings.FindAsync(id);
            ad.Title = model.Title;
            ad.Description = model.Description;
            ad.Price = model.Price;
            ad.CategoryId = model.CategoryId;
            ad.ImageUrl = model.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(ad);
        }

        // 📌 İlan silme (sadece ilan sahibi yapabilir)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAd(int id)
        {
            if (!await IsOwner(id))
                return Forbid(); // Kullanıcının ilanı değilse işlem yapamaz

            var ad = await _context.AdListings.FindAsync(id);
            _context.AdListings.Remove(ad);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan silindi!" });
        }
    }
}