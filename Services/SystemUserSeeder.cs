using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Seeder tạo tài khoản admin mặc định (MTTQAdmin) nếu chưa tồn tại.
    /// </summary>
    public static class SystemUserSeeder
    {
        public static async Task SeedAsync(DataMTTQContext context)
        {
            // Kiểm tra xem đã có tài khoản MTTQAdmin chưa
            var existingAdmin = await context.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenDangNhap == "MTTQAdmin");

            if (existingAdmin != null)
            {
                return; // Đã tồn tại, không tạo lại
            }

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

            // Tạo tài khoản admin với mật khẩu đã hash
            var adminUser = new NguoiDung
            {
                TenDangNhap = "MTTQAdmin",
                MatKhau = PasswordHelper.HashPassword("Aa12345678"),
                HoTen = "Quản trị viên hệ thống",
                Email = null,
                SoDienThoai = null,
                IdvaiTro = adminRole.IdvaiTro,
                TrangThai = "HoatDong",
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now
            };

            context.NguoiDungs.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}