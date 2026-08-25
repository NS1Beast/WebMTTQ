using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using WebMTTQ.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// 1. Đọc chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// 2. Cấu hình DbContext với chuỗi kết nối
builder.Services.AddDbContext<DataMTTQContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// 3. Đăng ký Data Protection để mã hóa các giá trị nhạy cảm
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Keys")));

// 4. Đăng ký System Settings Service
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();

// 4.1. Đăng ký Email Service (gửi OTP)
builder.Services.AddScoped<IEmailService, EmailService>();

// 4.2. Đăng ký Quyền Truy Cập Service
builder.Services.AddScoped<IQuyenTruyCapService, QuyenTruyCapService>();

// 4.3. Đăng ký Thống Kê Truy Cập Service
builder.Services.AddScoped<ITruyCapService, TruyCapService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// Thêm Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".MTTQAdmin.Session";
    // Phase 5 - session cookie hardening (environment-aware).
    // SameSite=Lax keeps the login/redirect flow working; SecurePolicy=SameAsRequest
    // flags the cookie Secure when served over HTTPS (production) but still allows the
    // cookie over plain HTTP in local Development, so login is not broken on HTTP dev.
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Phase 5: apply security response headers to every request (incl. static files & errors).
app.UseSecurityHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Phục vụ file tĩnh (bao gồm file upload runtime trong wwwroot/uploads)
app.UseRouting();

// Bắt buộc phải có UseSession() sau UseRouting() và trước UseAuthorization()
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 5. Apply EF migrations + Seed configuration keys at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataMTTQContext>();
//    await context.Database.MigrateAsync();
    await SystemSettingsSeeder.SeedAsync(context);
    await SystemUserSeeder.SeedAsync(context);
}

app.Run();
