namespace E_commerce.Data.Entities
{
    public class ImagePath
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
