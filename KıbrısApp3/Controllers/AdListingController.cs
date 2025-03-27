// AdListingController.cs
using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KıbrısApp3.DTO;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace KıbrısApp3.Controllers
{

    [Route("api/ad-listings")]
    [ApiController]
    public class AdListingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
       
        private readonly Cloudinary _cloudinary; // 👈 bunu EKLE


        public AdListingController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;

            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchAds(
    [FromQuery] string? keyword,
    [FromQuery] string? categoryName,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] string? sortBy)
        {
            var query = _context.AdListings
                                .Include(a => a.Category)
                                    .ThenInclude(c => c.ParentCategory)
                                .Include(a => a.Images)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.Title.Contains(keyword) || a.Description.Contains(keyword));
            }

            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(a => a.Category.Name.ToLower().Contains(categoryName.ToLower()));
            }

            if (minPrice.HasValue)
                query = query.Where(a => a.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(a => a.Price <= maxPrice.Value);

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

            var rawAds = await query.ToListAsync();

            // ✅ Kategori yolunu çözümleyen yardımcı fonksiyon
            List<string> BuildCategoryPath(Category? category)
            {
                var path = new List<string>();
                while (category != null)
                {
                    path.Insert(0, category.Name); // en üste ekler
                    category = category.ParentCategory;
                }
                return path;
            }

            // ✅ DTO olarak dönüştür
            var ads = rawAds.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.Price,
                a.Address,
                a.Status,
                CategoryId = a.Category.Id,
                CategoryName = a.Category.Name,
                CategoryPath = BuildCategoryPath(a.Category), // 👈 burası 🔥
                Images = a.Images.Select(i => new { i.Url }).ToList()
            }).ToList();

            return Ok(ads);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdById(int id)
        {
            var ad = await _context.AdListings
                .Include(a => a.Images)         // 👈 Tüm resimleri dahil et
                .Include(a => a.Category)       // 👈 Kategori bilgisi
                .Include(a => a.User)           // 👈 Kullanıcı bilgisi
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad == null)
                return NotFound(new { message = "İlan bulunamadı!" });

            return Ok(new
            {
                ad.Id,
                ad.Title,
                ad.Description,
                ad.Price,
                ad.Status,
                ad.Address,
                ad.Latitude,
                ad.Longitude,
                ad.CategoryId,
                CategoryName = ad.Category?.Name,
                ad.UserId,
                SellerName = ad.User?.FullName,
                ImageUrls = ad.Images.Select(img => img.Url).ToList()
            });
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
            // 🧠 Tüm kategorileri alıyoruz
            var allCategories = await _context.Categories.ToListAsync();

            // 🧠 Alt kategoriler dahil tüm id'leri bul
            var categoryIds = GetAllSubCategoryIds(categoryId, allCategories);

            // 🎯 Bu id’leri kullanarak filtreleme yap
            var ads = await _context.AdListings
                                     .Include(a => a.Category)
                                     .Include(a => a.User)
                                     .Where(a => categoryIds.Contains(a.CategoryId))
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
                                         SellerName = a.User.FullName,
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
                ImageUrl = "" // İlk fotoğraf buraya atanacak
            };

            // ✅ Cloudinary üzerinden görsel yükleme
            if (model.Base64Images != null && model.Base64Images.Count > 0)
            {
                for (int i = 0; i < model.Base64Images.Count; i++)
                {
                    var base64 = model.Base64Images[i];
                    string actualBase64 = base64;

                    if (base64.Contains(","))
                    {
                        var parts = base64.Split(',');
                        if (parts.Length == 2)
                            actualBase64 = parts[1];
                    }

                    try
                    {
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription($"image_{Guid.NewGuid()}", new MemoryStream(Convert.FromBase64String(actualBase64))),
                            Folder = "ad-listings"
                        };

                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var imageUrl = uploadResult.SecureUrl.ToString();

                            if (i == 0)
                                ad.ImageUrl = imageUrl;

                            ad.Images ??= new List<AdImage>();
                            ad.Images.Add(new AdImage { Url = imageUrl });
                        }
                        else
                        {
                            return StatusCode(500, new { message = "Cloudinary yükleme başarısız!" });
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { message = "Görsel yüklenirken hata oluştu!", error = ex.Message });
                    }
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
        // 📌 Kategorinin tüm alt kategori ID’lerini (recursive) bulan yardımcı metot
        private List<int> GetAllSubCategoryIds(int categoryId, List<Category> allCategories)
        {
            List<int> ids = new List<int> { categoryId };

            var children = allCategories
                            .Where(c => c.ParentCategoryId == categoryId)
                            .ToList();

            foreach (var child in children)
            {
                ids.AddRange(GetAllSubCategoryIds(child.Id, allCategories));
            }

            return ids;
        }

    }

}