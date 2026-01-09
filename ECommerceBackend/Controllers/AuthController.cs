using Microsoft.AspNetCore.Mvc;
using ECommerceBackend.Data;
using ECommerceBackend.Models;
using ECommerceBackend.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")] //元數據 (Metadata)
    [ApiController] 
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; // 我們需要讀取 appsettings.json 裡的 Key

        // 建構子：注入資料庫 (context) 和 設定檔 (configuration)
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. 註冊功能
        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDto request)
        {
            // 檢查是否重複註冊
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("使用者名稱已存在");
            }

            // 建立使用者 (密碼加密)
            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("註冊成功！");
        }

        // 2. 登入功能 (這裡就是產生 JWT 的地方！)
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto request)
        {
            // A. 找人：去資料庫找這個使用者
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            Console.WriteLine($"前端傳來的密碼: {request.Password}");
            Console.WriteLine($"資料庫裡的雜湊: {user.PasswordHash}");

            // B. 驗證：如果找不到人，或是密碼對不上 (Verify)
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return BadRequest("帳號或密碼錯誤");
            }
            


            // C. 發卡：帳密正確，開始製作 JWT Token
            string token = CreateToken(user);

            // 回傳 Token 給前端
            return Ok(token);
        }

        // --- 私有方法：負責產生 JWT 的工廠 ---
        private string CreateToken(User user)
        {
            var secretKey = _configuration.GetSection("JwtSettings:Key").Value
            ?? throw new InvalidOperationException("設定檔找不到 JwtSettings:Key");
            // 1. 設定「聲明 (Claims)」：你想在票裡面寫什麼資訊？
            // 這些資訊解碼後大家都看得到，千萬不要放密碼！
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // 放使用者名稱
                new Claim(ClaimTypes.Role, user.Role)      // 放角色 (Admin/User)
            };

            // 2. 準備「印章 (Key)」：從設定檔拿那串長長的字串
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 3. 簽名憑證 (Credentials)：指定要用這把 Key 和 HmacSha512 演算法來簽名
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // 4. 製作 Token (Payload)
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1), // 設定過期時間 (例如 1 天後過期)
                signingCredentials: creds
            );

            // 5. 寫出 Token 字串
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}