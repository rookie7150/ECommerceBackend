using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceBackend.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        // 1. 這是誰的購物車？ (連結 User)
        // 這裡我們只存 ID 就可以了，因為通常是 "用 ID 去找購物車"
        public int UserId { get; set; }

        // 2. 這是哪個商品？ (連結 Product)
        public int ProductId { get; set; }

        // 👇 導覽屬性 (Navigation Property)
        // 加上這行，EF Core 會自動幫我們去 Product 表抓商品的詳細資料 (名字、價格)
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        // 3. 買了幾個？
        [Range(1, 100, ErrorMessage = "數量必須至少為 1")]
        public int Quantity { get; set; }
    }
}