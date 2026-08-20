using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Seeder đảm bảo vai trò Admin hệ thống tồn tại.
    /// KHÔNG tạo tài khoản admin mặc định - tài khoản admin chính sẽ được tạo
    /// qua trang đăng ký tài khoản admin khi hệ thống chạy lần đầu.
    /// </summary>
    public static class SystemUserSeeder
    {
        public static async Task SeedAsync(DataMTTQContext context)
        {
            // Tìm vai trò Quản trị viên (Admin) - có toàn quyền
            // Chỉ khớp CHÍNH XÁC tên "Quản trị viên" hoặc "Admin" (không phân biệt hoa thường)
            var allRoles = await context.VaiTros.AsNoTracking().ToListAsync();
            var adminRole = allRoles.FirstOrDefault(v => QuyenHelper.IsAdminVaiTro(v.TenVaiTro));

            // Nếu không có vai trò Admin, tạo mới
            if (adminRole == null)
            {
                adminRole = new VaiTro
                {
                    TenVaiTro = "Quản trị viên",
                    QuyenHan = QuyenBitmask.ToanQuyen, // 15 = toàn quyền
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now
                };
                context.VaiTros.Add(adminRole);
                await context.SaveChangesAsync();
            }
        }
    }
}