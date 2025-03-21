using KıbrısApp3.Data;
using KıbrısApp3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KıbrısApp3.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                                           .Select(c => new
                                           {
                                               c.Id,
                                               c.Name,
                                               c.ParentCategoryId
                                           })
                                           .ToListAsync();

            return Ok(categories);
        }



        [HttpPost("seed")]
        public async Task<IActionResult> SeedCategories()
        {
            try
            {
                Console.WriteLine("🔹 Kategori ekleme işlemi başladı...");

                if (!_context.Categories.Any())
                {
                    Console.WriteLine("🔹 Veritabanında hiç kategori yok, ekleniyor...");

                    // 📌 Önce ANA KATEGORİLERİ ekleyelim
                    var mainCategories = new List<Category>
            {
                new Category { Name = "Vasıta" },
                new Category { Name = "Telefon & Elektronik" },
                new Category { Name = "Ev & Yaşam" },
                new Category { Name = "Giyim & Aksesuar" },
                new Category { Name = "Kişisel Bakım" },
                new Category { Name = "Diğer" }
            };

                    await _context.Categories.AddRangeAsync(mainCategories);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("✅ Ana kategoriler eklendi.");

                    // 📌 Şimdi ALT KATEGORİLERİ ekleyelim (Ana kategorileri ID'ye göre çekelim)
                    var vasita = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Vasıta");
                    var elektronik = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Telefon & Elektronik");
                    var evYasam = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Ev & Yaşam");
                    var giyim = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Giyim & Aksesuar");
                    var kisiselBakim = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Kişisel Bakım");
                    var diger = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Diğer");

                    var subCategories = new List<Category>
            {
                // 📌 Vasıta Alt Kategorileri
                new Category { Name = "Otomobil", ParentCategoryId = vasita.Id },
                new Category { Name = "Arazi-SUV-Pick-Up", ParentCategoryId = vasita.Id },
                new Category { Name = "Motosiklet", ParentCategoryId = vasita.Id },
                new Category { Name = "ATV-UTV", ParentCategoryId = vasita.Id },
                new Category { Name = "Karavan", ParentCategoryId = vasita.Id },

                // 📌 Elektronik Alt Kategorileri
                new Category { Name = "iPhone", ParentCategoryId = elektronik.Id },
                new Category { Name = "Samsung", ParentCategoryId = elektronik.Id },
                new Category { Name = "Xiaomi", ParentCategoryId = elektronik.Id },
                new Category { Name = "Huawei", ParentCategoryId = elektronik.Id },
                new Category { Name = "Tablet", ParentCategoryId = elektronik.Id },

                // 📌 Ev & Yaşam Alt Kategorileri
                new Category { Name = "Mobilya", ParentCategoryId = evYasam.Id },
                new Category { Name = "Mutfak Gereçleri", ParentCategoryId = evYasam.Id },
                new Category { Name = "Beyaz Eşya", ParentCategoryId = evYasam.Id },

                // 📌 Giyim & Aksesuar Alt Kategorileri
                new Category { Name = "Kadın", ParentCategoryId = giyim.Id },
                new Category { Name = "Erkek", ParentCategoryId = giyim.Id },
                new Category { Name = "Çocuk", ParentCategoryId = giyim.Id },

                // 📌 Kişisel Bakım Alt Kategorileri
                new Category { Name = "Makyaj", ParentCategoryId = kisiselBakim.Id },
                new Category { Name = "Cilt Bakım", ParentCategoryId = kisiselBakim.Id },
                new Category { Name = "Saç Bakım", ParentCategoryId = kisiselBakim.Id },

                // 📌 Diğer Alt Kategoriler
                new Category { Name = "Kitap-Kırtasiye", ParentCategoryId = diger.Id },
                new Category { Name = "Hobi-Müzik", ParentCategoryId = diger.Id }
            };

                    await _context.Categories.AddRangeAsync(subCategories);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("✅ Alt kategoriler eklendi!");
                }
                else
                {
                    Console.WriteLine("⚠️ Kategoriler zaten mevcut, ekleme yapılmadı.");
                }

                return Ok(new { message = "Kategori ekleme işlemi tamamlandı." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Hata: {ex.Message}");
                return StatusCode(500, new { message = "Kategori ekleme hatası!", error = ex.Message });
            }
        }


    }
}
