using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authorization; // 👈 為了使用 [Authorize]

namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        // 建構子：把廚房服務生 (AppDbContext) 叫進來
        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // 1. 取得所有商品 (GET: api/Product)
        // 這個沒有 [Authorize]，代表「遊客」也可以看
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // 2. 取得單一商品 (GET: api/Product/5)
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound("找不到這項商品");
            }

            return product;
        }

        // 3. 上架新商品 (POST: api/Product)
        // 👇 重點！只有登入並帶著 Token 的人才能呼叫這裡
        [HttpPost]
        [Authorize] 
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            product.Id = 0;
            // 把商品加入「待辦清單」
            _context.Products.Add(product);
            
            // 真正寫入資料庫 (這時候才會產生 Id)
            await _context.SaveChangesAsync();

            // 回傳 201 Created，並告訴前端去哪裡看剛新增的商品
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
    }
}