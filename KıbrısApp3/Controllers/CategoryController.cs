﻿using KıbrısApp3.Data;
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
                .OrderBy(c => c.DisplayOrder)
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
            var result = BuildCategoryTree(root, root.IconUrl); // ✅ Buraya yazıyoruz

            return Ok(result);
        }

        private object BuildCategoryTreeWithIconInheritance(Category category, string inheritedIconUrl = null)
        {
            var icon = !string.IsNullOrEmpty(category.IconUrl)
                ? category.IconUrl
                : inheritedIconUrl;

            return new
            {
                id = category.Id,
                name = category.Name,
                iconUrl = icon,
                children = category.Children
                    .Select(child => BuildCategoryTreeWithIconInheritance(child, icon))
                    .ToList()
            };
        }


        [HttpGet("tree")]
        public async Task<IActionResult> GetFullCategoryTree()
        {
            var allCategories = await _context.Categories.ToListAsync();
            var categoryDict = allCategories.ToDictionary(c => c.Id);

            // Parent-child bağla
            foreach (var cat in allCategories)
            {
                if (cat.ParentCategoryId.HasValue &&
                    categoryDict.ContainsKey(cat.ParentCategoryId.Value))
                {
                    var parent = categoryDict[cat.ParentCategoryId.Value];
                    parent.Children.Add(cat);
                }
            }

            // En üst (root) kategorileri bul
            var rootCategories = allCategories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => BuildCategoryTreeWithIconInheritance(c, c.IconUrl)) // icon miras başlat

                .ToList();

            return Ok(rootCategories);
        }




        private object BuildCategoryTree(Category category, string inheritedIconUrl = null)
        {
            var icon = !string.IsNullOrEmpty(category.IconUrl)
                ? category.IconUrl
                : inheritedIconUrl;

            return new
            {
                id = category.Id,
                name = category.Name,
                iconUrl = icon,
                children = category.Children.Select(child => BuildCategoryTree(child, icon)).ToList()
            };
        }




        [HttpPost("seed")]
        public async Task<IActionResult> SeedCategories()
        {
            if (_context.Categories.Any())
                return Ok("Kategoriler zaten eklenmiş.");

            // Ana kategoriler
            var anaKategoriler = new List<Category>
    {
        new Category { Name = "Vasıta", IconUrl = "/icons/arac.png", DisplayOrder = 1 },
        new Category { Name = "Emlak", IconUrl = "/icons/emlak.png", DisplayOrder = 2 },
        new Category { Name = "Telefon", IconUrl = "/icons/telefon.png", DisplayOrder = 3 },
        new Category { Name = "Elektronik", IconUrl = "/icons/elektronik1.png", DisplayOrder = 4 },
        new Category { Name = "Ev & Yaşam", IconUrl = "/icons/evyasam.png", DisplayOrder = 5 },
        new Category { Name = "Giyim & Aksesuar", IconUrl = "/icons/giyim.png", DisplayOrder = 6 },
        new Category { Name = "Kişisel Bakım", IconUrl = "/icons/bakim.png", DisplayOrder = 7 },
        new Category { Name = "Diğer", IconUrl = "/icons/diger.png", DisplayOrder = 8 }
    };

            await _context.Categories.AddRangeAsync(anaKategoriler);
            await _context.SaveChangesAsync();

            // Ana kategori referansları
            var vasita = await _context.Categories.FirstAsync(c => c.Name == "Vasıta");
            var telefon = await _context.Categories.FirstAsync(c => c.Name == "Telefon");
            var elektronik = await _context.Categories.FirstAsync(c => c.Name == "Elektronik");
            var emlak = await _context.Categories.FirstAsync(c => c.Name == "Emlak");

            // Vasıta alt kategorileri
            var vasitaAltKategoriler = new List<Category>
    {
        new Category { Name = "Otomobil", ParentCategoryId = vasita.Id, IconUrl = "/icons/otomobil.png" },
        new Category { Name = "Arazi-SUV-Pick-Up", ParentCategoryId = vasita.Id, IconUrl = "/icons/suv.png" },
        new Category { Name = "Motosiklet", ParentCategoryId = vasita.Id, IconUrl = "/icons/motosiklet.png" },
        new Category { Name = "ATV-UTV", ParentCategoryId = vasita.Id, IconUrl = "/icons/utv.png" },
        new Category { Name = "Karavan", ParentCategoryId = vasita.Id, IconUrl = "/icons/karavan.png" }
    };

            var telefonAltKategoriler = new List<Category>
    {
        new Category { Name = "iPhone", ParentCategoryId = telefon.Id, IconUrl = "/icons/telefon.png" },
        new Category { Name = "Samsung", ParentCategoryId = telefon.Id, IconUrl = "/icons/telefon.png" },
        new Category { Name = "Xiaomi", ParentCategoryId = telefon.Id, IconUrl = "/icons/telefon.png" },
        new Category { Name = "Huawei", ParentCategoryId = telefon.Id, IconUrl = "/icons/telefon.png" }
    };

            var elektronikAltKategoriler = new List<Category>
    {
        new Category { Name = "Bilgisayar", ParentCategoryId = elektronik.Id, IconUrl = "/icons/masaustu.png" },
        new Category { Name = "Tablet", ParentCategoryId = elektronik.Id, IconUrl = "/icons/tablet.png" },
        new Category { Name = "Laptop", ParentCategoryId = elektronik.Id, IconUrl = "/icons/laptop.png" },
        new Category { Name = "Kulaklık", ParentCategoryId = elektronik.Id, IconUrl = "/icons/kulaklık.png" },
        new Category { Name = "Akıllı Saat", ParentCategoryId = elektronik.Id, IconUrl = "/icons/akıllı saat.png" },
        new Category { Name = "Oyun Konsolu", ParentCategoryId = elektronik.Id, IconUrl = "/icons/oyunkonsolu.png" },
        new Category { Name = "Televizyon", ParentCategoryId = elektronik.Id, IconUrl = "/icons/televizyon.png" },
        new Category { Name = "Diğer", ParentCategoryId = elektronik.Id, IconUrl = "/icons/elektronik-diger.png" }
    };

            var emlakAltKategoriler = new List<Category>
    {
        new Category { Name = "Konut", ParentCategoryId = emlak.Id, IconUrl = "/icons/konut.png" },
        new Category { Name = "İşyeri", ParentCategoryId = emlak.Id, IconUrl = "/icons/işyeri.png" },
        new Category { Name = "Arsa", ParentCategoryId = emlak.Id, IconUrl = "/icons/arsa.png" }
    };

            await _context.Categories.AddRangeAsync(vasitaAltKategoriler);
            await _context.Categories.AddRangeAsync(telefonAltKategoriler);
            await _context.Categories.AddRangeAsync(elektronikAltKategoriler);
            await _context.Categories.AddRangeAsync(emlakAltKategoriler);

            await _context.SaveChangesAsync();

            return Ok("Tüm kategoriler ve alt kategoriler, ikonlarıyla birlikte başarıyla kaydedildi.");
        }




    }
}
