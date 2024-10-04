using System;
using System.Collections.Generic;

namespace Webclient.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
            Payments = new HashSet<Payment>();
        }

        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
    public class OrderForGraph
    {
        public decimal TotalPrice { get; set; }
        public DateTime OrderTime { get; set; }
    }
}
