namespace KıbrısApp3.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; } // 📌 Alt kategori desteği
        public Category ParentCategory { get; set; }
        public List<Category> SubCategories { get; set; } = new List<Category>();
    }
}
