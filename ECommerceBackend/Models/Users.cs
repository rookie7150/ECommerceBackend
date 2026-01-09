namespace ECommerceBackend.Models
{
    public class User
    {
        public int Id { get; set; } 

        public required string Username { get; set; }

        public required string PasswordHash { get; set; } // 注意：這裡是存雜湊值，不是明碼
        
        public string Role { get; set; } = "User"; // 預設角色為一般使用者

    }
}