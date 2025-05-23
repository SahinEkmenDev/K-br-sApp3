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
        [HttpGet("{id}/subcategories")]
        public async Task<IActionResult> GetSubCategories(int id)
        {
            var subCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == id)
                .ToListAsync();

            return Ok(subCategories);
        }


        [HttpGet]
        public async Task<IActionResult> GetCategoryTree()
        {
            var categories = await _context.Categories.ToListAsync();

            var categoryDict = categories.ToDictionary(c => c.Id);

            foreach (var category in categories)
            {
                if (category.ParentCategoryId.HasValue &&
                    categoryDict.ContainsKey(category.ParentCategoryId.Value))
                {
                    var parent = categoryDict[category.ParentCategoryId.Value];
                    parent.Children.Add(category);
                }
            }

            // Sadece en üstteki (parent'ı olmayan) kategorileri döndürüyoruz
            var rootCategories = categories
                .Where(c => c.ParentCategoryId == null)
                .Select(c => BuildCategoryDto(c, c.IconUrl))

                .ToList();

            return Ok(rootCategories);
        }

        // DTO dönüşüm (category → nested object)
        private object BuildCategoryDto(Category category, string inheritedIconUrl = null)
        {
            var icon = !string.IsNullOrEmpty(category.IconUrl)
                ? category.IconUrl
                : inheritedIconUrl;

            return new
            {
                id = category.Id,
                name = category.Name,
                iconUrl = icon,
                children = category.Children.Select(c => BuildCategoryDto(c, icon)).ToList()
            };
        }

        [HttpGet("{id}/tree")]
        public async Task<IActionResult> GetCategoryTreeFromNode(int id)
        {
            var allCategories = await _context.Categories.ToListAsync();

            var categoryDict = allCategories.ToDictionary(c => c.Id);

            foreach (var cat in allCategories)
            {
                if (cat.ParentCategoryId.HasValue &&
                    categoryDict.ContainsKey(cat.ParentCategoryId.Value))
                {
                    var parent = categoryDict[cat.ParentCategoryId.Value];
                    parent.Children.Add(cat);
                }
            }

            if (!categoryDict.ContainsKey(id))
                return NotFound("Kategori bulunamadı.");

            var root = categoryDict[id];
            var result = BuildCategoryTree(root);

            return Ok(result);
        }

        private object BuildCategoryTree(Category category)
        {
            return new
            {
                id = category.Id,
                name = category.Name,
                children = category.Children.Select(BuildCategoryTree).ToList()
            };
        }



        [HttpPost("seed")]
        public async Task<IActionResult> SeedCategories()
        {
            if (_context.Categories.Any())
                return Ok("Kategoriler zaten eklenmiş.");

            // ✅ Ana kategoriler
            var anaKategoriler = new List<Category>
{
    new Category { Name = "Vasıta", IconUrl = "/icons/vasita.png" },
    new Category { Name = "Emlak", IconUrl = "/icons/emlak/emlak.png" },
    new Category { Name = "Telefon", IconUrl = "/icons/telefon.png" },
    new Category { Name = "Elektronik", IconUrl = "/icons/elektronik/elektronik.png" },
    new Category { Name = "Ev & Yaşam", IconUrl = "/icons/evyasam.png" },
    new Category { Name = "Giyim & Aksesuar", IconUrl = "/icons/giyim.png" },
    new Category { Name = "Kişisel Bakım", IconUrl = "/icons/bakim.png" },
    new Category { Name = "Diğer", IconUrl = "/icons/diger.png" }
};


            await _context.Categories.AddRangeAsync(anaKategoriler);
            await _context.SaveChangesAsync();

            // Ana kategori referansları
            var vasita = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Vasıta");
            var telefon = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Telefon");
            var elektronik = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Elektronik");
            var emlak = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Emlak");

            // ✅ Vasıta alt kategorileri
            var vasitaAltKategoriler = new List<Category>
    {
        new Category { Name = "Otomobil", ParentCategoryId = vasita.Id },
        new Category { Name = "Arazi-SUV-Pick-Up", ParentCategoryId = vasita.Id },
        new Category { Name = "Motosiklet", ParentCategoryId = vasita.Id },
        new Category { Name = "ATV-UTV", ParentCategoryId = vasita.Id },
        new Category { Name = "Karavan", ParentCategoryId = vasita.Id }
           };

            // ✅ Telefon alt kategorileri
            var telefonAltKategoriler = new List<Category>
    {
        new Category { Name = "iPhone", ParentCategoryId = telefon.Id },
        new Category { Name = "Samsung", ParentCategoryId = telefon.Id },
        new Category { Name = "Xiaomi", ParentCategoryId = telefon.Id },
        new Category { Name = "Huawei", ParentCategoryId = telefon.Id }
    };

            // ✅ Elektronik alt kategorileri
            var elektronikAltKategoriler = new List<Category>
    {
        new Category { Name = "Bilgisayar", ParentCategoryId = elektronik.Id },
        new Category { Name = "Tablet", ParentCategoryId = elektronik.Id },
        new Category { Name = "Laptop", ParentCategoryId = elektronik.Id },
        new Category { Name = "Kulaklık", ParentCategoryId = elektronik.Id },
        new Category { Name = "Akıllı Saat", ParentCategoryId = elektronik.Id },
        new Category { Name = "Oyun Konsolu", ParentCategoryId = elektronik.Id },
        new Category { Name = "Televizyon", ParentCategoryId = elektronik.Id },
        new Category { Name = "Diğer", ParentCategoryId = elektronik.Id }
    };

            // ✅ Emlak alt kategorileri
            var emlakAltKategoriler = new List<Category>
    {
        new Category { Name = "Konut", ParentCategoryId = emlak.Id },
        new Category { Name = "İşyeri", ParentCategoryId = emlak.Id },
        new Category { Name = "Arsa", ParentCategoryId = emlak.Id }
    };

            // Hepsini tek seferde ekle
            await _context.Categories.AddRangeAsync(vasitaAltKategoriler);
            await _context.Categories.AddRangeAsync(telefonAltKategoriler);
            await _context.Categories.AddRangeAsync(elektronikAltKategoriler);
            await _context.Categories.AddRangeAsync(emlakAltKategoriler);
            await _context.SaveChangesAsync();

            return Ok("Ana ve alt kategoriler başarıyla eklendi.");
        }


       
    }
}
