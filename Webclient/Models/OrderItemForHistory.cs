namespace Webclient.Models
{
    public class OrderItemForHistory
    {
        public int Quantity { get; set; }
        public ProductForHistory Product { get; set; }
        public Boolean HasReview { get; set; }
    }
}
