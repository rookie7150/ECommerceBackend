using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // 👈 記得引用這個

namespace ECommerceBackend.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        // 屬於哪一張訂單？
        public int OrderId { get; set; }
        
        // 避免轉 JSON 時發生無限迴圈，加上 JsonIgnore
        [JsonIgnore] 
        public Order? Order { get; set; }

        // 買了哪個商品？
        public int ProductId { get; set; }

        // 👇 歷史快照 (Snapshot)
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // 買了幾個？
        public int Quantity { get; set; }
    }
}