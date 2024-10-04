namespace Webclient.Models
{
    public class ReviewDTO
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string userFullname { get; set; }
    }
}
