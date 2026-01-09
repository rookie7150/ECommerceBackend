namespace ECommerceBackend.Dtos
{
    public class AddCartItemDto
    {
        // 使用者只需傳送商品 ID
        public int ProductId { get; set; }

        // 👇 關鍵在這裡！直接在這裡給它一個預設值 = 1
        // 如果前端沒傳 Quantity，它就會是 1。
        public int Quantity { get; set; } = 1;
    }
}