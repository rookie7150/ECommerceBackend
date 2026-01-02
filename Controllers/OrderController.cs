using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization;


namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 👈 一定要登入才能結帳
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // 1. 取得我的訂單列表 (GET: api/Order)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // 這裡用 Include 把訂單裡的明細 (Items) 也抓出來
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate) // 最新的訂單排前面
                .ToListAsync();
        }

        // 2. 結帳：把購物車變成訂單 (POST: api/Order/checkout)
        [HttpPost("checkout")]
        public async Task<ActionResult<Order>> Checkout()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // A. 把購物車裡的商品抓出來 (記得 Include Product 拿到價格)
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            // 防呆：如果購物車是空的，不能結帳
            if (cartItems.Count == 0)
            {
                return BadRequest("購物車是空的，無法結帳");
            }

            // B. 建立新訂單
            var order = new Order
            {
                UserId = user.Id,
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity) // 計算總價
            };

            // C. 【關鍵步驟】把購物車項目 (CartItem) 轉換成 訂單明細 (OrderItem)
            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    
                    // 👇 快照 (Snapshot)：把現在的價格和名稱存下來
                    // 這樣以後商品漲價，這張訂單的價格才不會變
                    Price = item.Product.Price, 
                    ProductName = item.Product.Name
                };
                order.Items.Add(orderItem);
            }

            // D. 存入訂單
            _context.Orders.Add(order);

            // E. 清空購物車 (因為已經買了)
            _context.CartItems.RemoveRange(cartItems);

            // F. 存檔 (Transaction: 要嘛全部成功，要嘛全部失敗)
            await _context.SaveChangesAsync();

            return Ok(new { message = "結帳成功！", orderId = order.Id });
        }

        // --- 私人小幫手 ---
        private async Task<User?> GetCurrentUserAsync()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}