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
        private readonly Cloudinary _cloudinary;

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
                .Include(a => a.Category).ThenInclude(c => c.ParentCategory)
                .Include(a => a.Images)
                .Include(a => a.User)
                .Include(a => a.CarDetail) // ✅ Car detayları dahil
                .Include(a => a.MotorcycleDetail)

                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(a => a.Title.Contains(keyword) || a.Description.Contains(keyword));

            if (!string.IsNullOrEmpty(categoryName))
                query = query.Where(a => a.Category.Name.ToLower().Contains(categoryName.ToLower()));

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

            List<string> BuildCategoryPath(Category? category)
            {
                var path = new List<string>();
                while (category != null)
                {
                    path.Insert(0, category.Name);
                    category = category.ParentCategory;
                }
                return path;
            }

            var ads = rawAds.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.Price,
                a.Currency,
                a.Address,
                a.Latitude,
                a.Longitude,
                a.Status,
                CategoryId = a.Category.Id,
                CategoryName = a.Category.Name,
                CategoryPath = BuildCategoryPath(a.Category),
                SellerName = a.User.FullName,
                UserId = a.UserId,
                Images = a.Images.Select(i => new { i.Url }).ToList(),
                CarDetail = a.CarDetail == null ? null : new
                {
                    a.CarDetail.Brand,
                    a.CarDetail.Model,
                    a.CarDetail.Year,
                    a.CarDetail.Kilometre,
                    a.CarDetail.HorsePower,
                    a.CarDetail.EngineSize,
                    a.CarDetail.BodyType,
                    a.CarDetail.Transmission,
                    a.CarDetail.FuelType
                },
                MotorcycleDetail = a.MotorcycleDetail == null ? null : new
                {
                    a.MotorcycleDetail.Brand,
                    a.MotorcycleDetail.Model,
                    a.MotorcycleDetail.Year,
                    a.MotorcycleDetail.Kilometre,
                    a.MotorcycleDetail.HorsePower,
                    a.MotorcycleDetail.EngineSize
                }

            }).ToList();

            return Ok(ads);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdById(int id)
        {
            var ad = await _context.AdListings
                .Include(a => a.Images)
                .Include(a => a.Category)
                .Include(a => a.User)
                .Include(a => a.CarDetail) // ✅ Eklendi
                .Include(a => a.MotorcycleDetail)

                .FirstOrDefaultAsync(a => a.Id == id);

            if (ad == null)
                return NotFound(new { message = "İlan bulunamadı!" });

            return Ok(new
            {
                ad.Id,
                ad.Title,
                ad.Description,
                ad.Price,
                ad.Currency,
                ad.Status,
                ad.Address,
                ad.Latitude,
                ad.Longitude,
                ad.CategoryId,
                CategoryName = ad.Category?.Name,
                ad.UserId,
                SellerName = ad.User?.FullName,
                ImageUrls = ad.Images.Select(img => img.Url).ToList(),
                CarDetail = ad.CarDetail == null ? null : new
                {
                    ad.CarDetail.Brand,
                    ad.CarDetail.Model,
                    ad.CarDetail.Year,
                    ad.CarDetail.Kilometre,
                    ad.CarDetail.HorsePower,
                    ad.CarDetail.EngineSize,
                    ad.CarDetail.BodyType,
                    ad.CarDetail.Transmission,
                    ad.CarDetail.FuelType
                },
                MotorcycleDetail = ad.MotorcycleDetail == null ? null : new
                {
                    ad.MotorcycleDetail.Brand,
                    ad.MotorcycleDetail.Model,
                    ad.MotorcycleDetail.Year,
                    ad.MotorcycleDetail.Kilometre,
                    ad.MotorcycleDetail.HorsePower,
                    ad.MotorcycleDetail.EngineSize
                }

            });
        }


        [HttpGet("user-ads")]
        [Authorize]
        public async Task<IActionResult> GetUserAds()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Kullanıcı bulunamadı." });

            var ads = await _context.AdListings
                .Include(a => a.Images)
                .Include(a => a.CarDetail) // ✅ Eklendi
                .Include(a => a.MotorcycleDetail)

                .Where(a => a.UserId == userId)
                .ToListAsync();

            var result = ads.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.Price,
                a.Currency,
                a.Status,
                a.Address,
                a.Latitude,
                a.Longitude,
                a.UserId,
                a.CategoryId,
                a.ImageUrl,
                ImageUrls = a.Images.Select(i => i.Url).ToList(),
                CarDetail = a.CarDetail == null ? null : new
                {
                    a.CarDetail.Brand,
                    a.CarDetail.Model,
                    a.CarDetail.Year,
                    a.CarDetail.Kilometre,
                    a.CarDetail.HorsePower,
                    a.CarDetail.EngineSize,
                    a.CarDetail.BodyType,
                    a.CarDetail.Transmission,
                    a.CarDetail.FuelType
                },
                MotorcycleDetail = a.MotorcycleDetail == null ? null : new
                {
                    a.MotorcycleDetail.Brand,
                    a.MotorcycleDetail.Model,
                    a.MotorcycleDetail.Year,
                    a.MotorcycleDetail.Kilometre,
                    a.MotorcycleDetail.HorsePower,
                    a.MotorcycleDetail.EngineSize
                }


            });

            return Ok(result);
        }


        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetAdsByCategory(int categoryId)
        {
            var allCategories = await _context.Categories.ToListAsync();
            var categoryIds = GetAllSubCategoryIds(categoryId, allCategories);

            var ads = await _context.AdListings
                .Include(a => a.Category)
                .Include(a => a.User)
                .Include(a => a.Images)
                .Include(a => a.CarDetail) // ✅ Eklendi
                .Include(a => a.MotorcycleDetail)

                .Where(a => categoryIds.Contains(a.CategoryId))
                .ToListAsync();

            var result = ads.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.Price,
                a.Currency,
                a.Status,
                a.Address,
                a.Latitude,
                a.Longitude,
                a.ImageUrl,
                a.CategoryId,
                CategoryName = a.Category.Name,
                a.UserId,
                SellerName = a.User.FullName,
                Images = a.Images.Select(i => i.Url).ToList(),
                CarDetail = a.CarDetail == null ? null : new
                {
                    a.CarDetail.Brand,
                    a.CarDetail.Model,
                    a.CarDetail.Year,
                    a.CarDetail.Kilometre,
                    a.CarDetail.HorsePower,
                    a.CarDetail.EngineSize,
                    a.CarDetail.BodyType,
                    a.CarDetail.Transmission,
                    a.CarDetail.FuelType
                },
                MotorcycleDetail = a.MotorcycleDetail == null ? null : new
                {
                    a.MotorcycleDetail.Brand,
                    a.MotorcycleDetail.Model,
                    a.MotorcycleDetail.Year,
                    a.MotorcycleDetail.Kilometre,
                    a.MotorcycleDetail.HorsePower,
                    a.MotorcycleDetail.EngineSize
                }

            });

            return Ok(result);
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
                Currency = model.Currency, // 💸 Eklendi
                CategoryId = model.CategoryId,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                UserId = userId,
                Status = model.Status ?? "Yayında",
                ImageUrl = ""
            };

            if (model.Base64Images != null && model.Base64Images.Count > 0)
            {
                for (int i = 0; i < model.Base64Images.Count; i++)
                {
                    var base64 = model.Base64Images[i];
                    string actualBase64 = base64.Contains(",") ? base64.Split(',')[1] : base64;

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

        [HttpPost("cars")]
        [Authorize]
        public async Task<IActionResult> AddCarAd([FromBody] CarAdCreateDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ad = new AdListing
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Currency = model.Currency,
                CategoryId = model.CategoryId,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                UserId = userId,
                Status = "Yayında",
                ImageUrl = ""
            };

            // Görsel yükleme
            if (model.Base64Images != null && model.Base64Images.Any())
            {
                ad.Images = new List<AdImage>();

                for (int i = 0; i < model.Base64Images.Count; i++)
                {
                    var base64 = model.Base64Images[i];
                    var actual = base64.Contains(",") ? base64.Split(',')[1] : base64;

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription($"car-{Guid.NewGuid()}", new MemoryStream(Convert.FromBase64String(actual))),
                        Folder = "car-ads"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var url = uploadResult.SecureUrl.ToString();

                    if (i == 0)
                        ad.ImageUrl = url;

                    ad.Images.Add(new AdImage { Url = url });
                }
            }

            _context.AdListings.Add(ad);
            await _context.SaveChangesAsync();

            var carDetail = new CarAdDetail
            {
                AdListingId = ad.Id,
                Brand = model.Brand,
                Model = model.Model,
                Year = model.Year,
                Kilometre = model.Kilometre,
                HorsePower = model.HorsePower,
                EngineSize = model.EngineSize,
                BodyType = model.BodyType,
                Transmission = model.Transmission,
                FuelType = model.FuelType
            };

            _context.CarAdDetails.Add(carDetail);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Araç ilanı başarıyla eklendi!", ad });
        }

        [HttpPost("motorcycles")]
        [Authorize]
        public async Task<IActionResult> AddMotorcycleAd([FromBody] MotorcycleAdCreateDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ad = new AdListing
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Currency = model.Currency,
                CategoryId = model.CategoryId,
                Address = model.Address,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                UserId = userId,
                Status = "Yayında",
                ImageUrl = ""
            };

            if (model.Base64Images != null && model.Base64Images.Any())
            {
                ad.Images = new List<AdImage>();

                for (int i = 0; i < model.Base64Images.Count; i++)
                {
                    var base64 = model.Base64Images[i];
                    var actual = base64.Contains(",") ? base64.Split(',')[1] : base64;

                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription($"moto-{Guid.NewGuid()}", new MemoryStream(Convert.FromBase64String(actual))),
                        Folder = "motorcycle-ads"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    var url = uploadResult.SecureUrl.ToString();

                    if (i == 0)
                        ad.ImageUrl = url;

                    ad.Images.Add(new AdImage { Url = url });
                }
            }

            _context.AdListings.Add(ad);
            await _context.SaveChangesAsync();

            var detail = new MotorcycleAdDetail
            {
                AdListingId = ad.Id,
                Brand = model.Brand,
                Model = model.Model,
                Year = model.Year,
                Kilometre = model.Kilometre,
                HorsePower = model.HorsePower,
                EngineSize = model.EngineSize
            };

            _context.MotorcycleAdDetails.Add(detail);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Motosiklet ilanı başarıyla eklendi!", ad });
        }



        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAd(int id, [FromBody] AdListing model)
        {
            if (!await IsOwner(id))
                return Forbid();

            var ad = await _context.AdListings.FindAsync(id);
            ad.Title = model.Title;
            ad.Description = model.Description;
            ad.Price = model.Price;
            ad.Currency = model.Currency; // 💸 Güncellenebilir
            ad.CategoryId = model.CategoryId;
            ad.ImageUrl = model.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(ad);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAd(int id)
        {
            if (!await IsOwner(id))
                return Forbid();

            var ad = await _context.AdListings.FindAsync(id);
            _context.AdListings.Remove(ad);
            await _context.SaveChangesAsync();

            return Ok(new { message = "İlan silindi!" });
        }

        private async Task<bool> IsOwner(int adId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            var ad = await _context.AdListings.FindAsync(adId);
            if (ad == null)
                return false;

            return ad.UserId == userId;
        }

        private List<int> GetAllSubCategoryIds(int categoryId, List<Category> allCategories)
        {
            List<int> ids = new List<int> { categoryId };
            var children = allCategories.Where(c => c.ParentCategoryId == categoryId).ToList();
            foreach (var child in children)
                ids.AddRange(GetAllSubCategoryIds(child.Id, allCategories));
            return ids;
        }
    }
}
