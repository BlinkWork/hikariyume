namespace Webclient.Models
{
    public class OrderForHistory
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Address { get; set; } 
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<OrderItemForHistory> orderItemForHistories { get; set; }
    }
}
