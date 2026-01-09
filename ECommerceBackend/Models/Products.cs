using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceBackend.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // 商品名稱

        public string Description { get; set; } = string.Empty; // 商品描述

        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; } // 價格 (注意：跟錢有關的一律用 decimal)

        public string? ImageUrl { get; set; }

        public string Category { get; set; } = string.Empty; // 分類 (先用字串簡單做)

    }
}