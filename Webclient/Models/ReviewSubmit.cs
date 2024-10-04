﻿namespace Webclient.Models
{
    public class ReviewSubmit
    {
        public int? UserId { get; set; }
        public int? ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
