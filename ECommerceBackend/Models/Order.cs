using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceBackend.Models
{
    public class Order
    {
        public int Id { get; set; }

        // 1. 這是誰下的單？
        public int UserId { get; set; }

        // 2. 什麼時候下的單？
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // 3. 總金額 (這筆訂單收了多少錢)
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        // 訂單狀態
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // 4. 包含哪些商品明細？ (一對多關係)
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}