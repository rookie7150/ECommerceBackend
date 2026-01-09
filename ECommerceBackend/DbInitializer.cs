using System;
using System.Linq;
using ECommerceBackend.Data; // ★★★ 加入這行！(對應剛剛看到的 namespace)
using ECommerceBackend.Models; // 如果 User 物件找不到，可能也要加這行

public static class DbInitializer
{
    // 傳入你的 DbContext 或是 UserManager (如果你用 Identity)
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // 檢查是否已經有任何使用者 (如果已經有，就不做動作)
        if (context.Users.Any())
        {
            return; 
        }

        // 建立預設管理員
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Admin"

        };

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}