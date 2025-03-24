// AdListingController.cs
using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KıbrısApp3.DTO;

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

        [HttpGet("search")]
        public async Task<IActionResult> SearchAds(
      [FromQuery] string? keyword,
      [FromQuery] string? categoryName, // ✅ categoryId yerine string olarak kategori adı
      [FromQuery] decimal? minPrice,
      [FromQuery] decimal? maxPrice,
      [FromQuery] string? sortBy)
        {
            var query = _context.AdListings
                                .Include(a => a.Category)
                                .AsQueryable();

            // 📌 Anahtar kelime
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Description.Contains(keyword));
            }

            // ✅ Kategori adına göre filtreleme
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(a => a.Category.Name.ToLower().Contains(categoryName.ToLower()));
            }

            // 📌 Fiyat filtreleri
            if (minPrice.HasValue)
                query = query.Where(a => a.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.Price <= maxPrice.Value);

            // 📌 Sıralama
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
                    Console.WriteLine("⚠ Kullanıcı ID bulunamadı!");
                    return Unauthorized(new { message = "Kullanıcı bulunamadı." });
                }

                var ads = await _context.AdListings
                          .Include(a => a.Images) // 👈 AdImage tablosunu dahil et
                          .Where(a => a.UserId == userId)
                          .ToListAsync();


                if (ads == null || ads.Count == 0)
                {
                    Console.WriteLine("⚠ Kullanıcının hiç ilanı yok!");
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


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAd([FromBody] AdListingCreateDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ad = new AdListing
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                CategoryId = model.CategoryId,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                UserId = userId,
                Status = model.Status ?? "Yayında",
                ImageUrl = "" // 👈 ilk foto buraya atanabilir
            };

            // Fotoğrafları işleyelim
            if (model.Base64Images != null && model.Base64Images.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // wwwroot/uploads yoksa oluştur
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Fotoğrafları kaydet
                for (int i = 0; i < model.Base64Images.Count; i++)
                {
                    var base64 = model.Base64Images[i];
                    var fileName = $"{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    var bytes = Convert.FromBase64String(base64.Split(',')[1]);

                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                    var imagePath = "/uploads/" + fileName;

                    if (i == 0)
                        ad.ImageUrl = imagePath;

                    ad.Images ??= new List<AdImage>();
                    ad.Images.Add(new AdImage { Url = imagePath });
                }

            }

            _context.AdListings.Add(ad);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan başarıyla eklendi!", ad });
        }






        private async Task<bool> IsOwner(int adId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("⚠ Kullanıcı ID bulunamadı!");
                return false;
            }

            var ad = await _context.AdListings.FindAsync(adId);
            if (ad == null)
            {
                Console.WriteLine($"⚠ İlan bulunamadı! ID: {adId}");
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