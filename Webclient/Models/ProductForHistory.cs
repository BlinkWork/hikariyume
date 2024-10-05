namespace Webclient.Models
{
    public class ProductForHistory
    {
        public int ProductId { get; set; }
        public string Image { get; set; }
        public string Name { get; set; } 
        public string? Size { get; set; }
        public string? Origin { get; set; }
        public string? Color { get; set; }
        public string? Age { get; set; }
        public string? Material { get; set; }
        public decimal Price { get; set; }
    }
}
