using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 👇 門神擋在這裡！只有 Token 裡 Role 是 "Admin" 的人才能進來
    [Authorize(Roles = "Admin")] 
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 1. 老闆要看「所有」訂單 (包含是誰買的)
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.Items) // 包含商品明細
                .OrderByDescending(o => o.OrderDate) // 新的在上面
                .ToListAsync();
        }

        // 2. 老闆要出貨 (修改訂單狀態)
        [HttpPut("orders/{id}/ship")]
        public async Task<IActionResult> ShipOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound("找不到這筆訂單");

            // 修改狀態
            order.Status = OrderStatus.Shipped;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"訂單 {id} 已出貨！" });
        }
    }
}