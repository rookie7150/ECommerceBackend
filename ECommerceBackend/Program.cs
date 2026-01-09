using ECommerceBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 記得加這行引用
using Microsoft.IdentityModel.Tokens; // 還有這行
using System.Text;
using Microsoft.OpenApi.Models;
using ECommerceBackend.Middleware;

var builder = WebApplication.CreateBuilder(args); 

// ==========================================
// 1. 服務註冊區 (Builder)
// ==========================================

// 加入 Controllers 功能
builder.Services.AddControllers();

// 加入 Swagger 相關服務
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    // 設定 Swagger 文件的標題與版本 (選用)
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API", Version = "v1" });

    // A. 告訴 Swagger 我們要用 "Bearer" 類型的 Token (定義鎖頭)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // B. 告訴 Swagger 所有的 API 都需要通過這個驗證 (把鎖頭掛上去)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// 1. 設定 JWT 驗證服務
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration.GetSection("JwtSettings:Key").Value 
        ?? throw new InvalidOperationException("設定檔找不到 JwtSettings:Key");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false, // 練習階段先簡化，不驗證發行者
            ValidateAudience = false // 練習階段先簡化，不驗證接收者
        };
    });

// 【關鍵】註冊 MS SQL Server 連線
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==========================================
// 2. 應用程式建立 (Build)
// ==========================================
var app = builder.Build();

// ==========================================
// 3. 請求管線設定 (Middleware)
// ==========================================

app.UseMiddleware<ExceptionMiddleware>();

// 開發環境開啟 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 自動執行資料庫遷移
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        //這行會檢查資料庫有沒有建立，沒有的話就照著你的 Model 自動建立 (包含 Migration)
        context.Database.Migrate(); 
        Console.WriteLine("資料庫遷移成功！(Database migrated successfully)");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "資料庫遷移失敗 (An error occurred while migrating the database.)");
    }
}

// 初始化資料庫 (建立預設管理員帳號等)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        DbInitializer.Initialize(context); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

// ... 中間是 swagger 設定 ...
app.UseHttpsRedirection();

app.UseAuthentication(); // 👈 先驗證身分 (你是誰？)
app.UseAuthorization(); // 再確認權限 (你可以做什麼？)

app.MapControllers();

app.Run();