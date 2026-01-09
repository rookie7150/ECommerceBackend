using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.Dtos; // 記得引用 DTO
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 👈 只有登入的人才能用購物車！
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // 1. 取得我的購物車 (GET: api/Cart)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetMyCart()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // 這裡使用了 Include("Product")，這樣回傳時才看得到商品名字，而不只是 ProductId
            return await _context.CartItems
                .Include(c => c.Product) 
                .Where(c => c.UserId == user.Id)
                .ToListAsync();
        }

        // 2. 加入購物車 (POST: api/Cart)
        [HttpPost]
        public async Task<ActionResult<string>> AddToCart(AddCartItemDto request)
        {
            // A. 確認使用者是誰
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            // B. 檢查購物車裡是不是已經有這個商品了？
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.ProductId == request.ProductId);

            if (existingItem != null)
            {
                // C1. 如果有，就「增加數量」 (例如原本 1 個，變 2 個)
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                // C2. 如果沒有，就「新增一筆」
                var newItem = new CartItem
                {
                    UserId = user.Id,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity // 這裡會拿到 DTO 的預設值 1
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok("商品已加入購物車");
        }

        // --- 私人小幫手：取得目前登入的使用者 ---
        private async Task<User?> GetCurrentUserAsync()
        {
            // 從 Token (User.Identity.Name) 拿到帳號
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            // 去資料庫找這個人
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}