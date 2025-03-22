namespace KıbrısApp3.DTO
{
    public class AdListingCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public string Status { get; set; }
    }

}
