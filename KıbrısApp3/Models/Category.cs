using System.ComponentModel.DataAnnotations.Schema;

namespace KıbrısApp3.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }

        public Category ParentCategory { get; set; }

        [NotMapped] // 🚨 Veritabanına kaydedilmesin!
        public List<Category> Children { get; set; } = new();
        public string? IconUrl { get; set; } // ✅ yeni alan
    }
}
