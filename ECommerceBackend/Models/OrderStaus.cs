namespace ECommerceBackend.Models
{
    // 用 Enum 可以避免打錯字，例如把 "Shipped" 打成 "Shiped"
    public enum OrderStatus
    {
        Pending = 0,   // 待處理 (預設)
        Shipped = 1,   // 已出貨
        Completed = 2, // 已完成
        Cancelled = 3  // 已取消
    }
}