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
    // GET: api/Product
    [HttpGet]
    public async Task<ActionResult<object>> GetProducts(
        [FromQuery] string? keyword,  // 搜尋關鍵字 (可不填)
        [FromQuery] decimal? minPrice, // 最低價格 (可不填)
        [FromQuery] decimal? maxPrice, // 最高價格 (可不填)
        [FromQuery] int page = 1,      // 第幾頁 (預設第 1 頁)
        [FromQuery] int pageSize = 5   // 一頁幾筆 (預設 5 筆)
    )
        {
            // 1. 起手式：先把資料表變成 "可查詢物件" (IQueryable)
            // 注意：這時候還沒有去資料庫撈資料喔！這只是在準備寫 SQL 指令。
            var query = _context.Products.AsQueryable();

            // 2. 搜尋邏輯：如果有給關鍵字，就過濾名稱或描述
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword));
            }

            // 3. 價格篩選：如果有給最低價/最高價
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. 統計總筆數 (這對前端做分頁按鈕很重要)
            var totalCount = await query.CountAsync();

            // 5. 分頁邏輯 (最關鍵的一步！)
            // Skip(5) = 跳過前 5 筆
            // Take(5) = 抓取接下來的 5 筆
            // 這樣就等於「第 2 頁」
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); // 👈 直到這裡，程式才會真的去 SQL Server 執行查詢

            // 6. 回傳包含分頁資訊的結果
            return Ok(new
            {
                TotalCount = totalCount, // 總共有幾筆符合
                Page = page,             // 目前在第幾頁
                PageSize = pageSize,     // 一頁顯示幾筆
                Data = products          // 真正的商品資料
            });
        }
        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // 3. 上架新商品 (POST: api/Product)
        // 👇 重點！只有登入並帶著 Token 的人才能呼叫這裡
        [HttpPost]
        [Authorize(Roles = "Admin")] 
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if(id != product.Id)
            {
                return BadRequest("商品 ID 不匹配");
            }
  
            _context.Entry(product).State = EntityState.Modified;
            

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent(); // 204 No Content 代表成功但不用回傳東西
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "商品已刪除" });
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

    }
}