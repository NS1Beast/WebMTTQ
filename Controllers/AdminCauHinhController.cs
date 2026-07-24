using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Controllers
{
    // BỔ SUNG ROUTE Ở ĐÂY ĐỂ TRÁNH LỖI NOT FOUND 404
    [Route("AdminCauHinh")]
    public class AdminCauHinhController : BaseAdminController // Nếu bạn không dùng BaseAdminController thì đổi thành Controller nhé
    {
        private readonly DataMTTQContext _context;

        public AdminCauHinhController(DataMTTQContext context)
        {
            _context = context;
        }

        // HÀM HIỂN THỊ TRANG CÀI ĐẶT
        [Route("")]
        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy toàn bộ cấu hình từ DB lên
            var configs = await _context.CauHinhHeThongs.ToListAsync();

            // Gán dữ liệu
            var model = new CauHinhViewModel
            {
                TenCoQuan = GetConfigValue(configs, "TenCoQuan"),
                DiaChi = GetConfigValue(configs, "DiaChi"),
                SoDienThoai = GetConfigValue(configs, "SoDienThoai"),
                Email = GetConfigValue(configs, "Email"),
                GioLamViec = GetConfigValue(configs, "GioLamViec"),
                LinkFacebook = GetConfigValue(configs, "LinkFacebook"),
                LinkZalo = GetConfigValue(configs, "LinkZalo"),
                BaoTriHeThong = GetConfigValue(configs, "BaoTriHeThong") == "1"
            };

            return View("~/Views/Admin/CauHinh/Index.cshtml", model);
        }

        // HÀM XỬ LÝ KHI BẤM NÚT "LƯU TẤT CẢ CÀI ĐẶT"
        [Route("")]
        [Route("Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CauHinhViewModel model)
        {
            if (ModelState.IsValid)
            {
                SetConfigValue("TenCoQuan", model.TenCoQuan, "Tên cơ quan / tổ chức");
                SetConfigValue("DiaChi", model.DiaChi, "Địa chỉ trụ sở");
                SetConfigValue("SoDienThoai", model.SoDienThoai, "Số điện thoại liên hệ đường dây nóng");
                SetConfigValue("Email", model.Email, "Hộp thư điện tử tiếp nhận");
                SetConfigValue("GioLamViec", model.GioLamViec, "Giờ làm việc hành chính");
                SetConfigValue("LinkFacebook", model.LinkFacebook, "Đường dẫn Fanpage Facebook");
                SetConfigValue("LinkZalo", model.LinkZalo, "Đường dẫn Zalo OA");
                SetConfigValue("BaoTriHeThong", model.BaoTriHeThong ? "1" : "0", "Chế độ bảo trì hệ thống (1=Bật, 0=Tắt)");

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã lưu cài đặt hệ thống thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/CauHinh/Index.cshtml", model);
        }

        // --- CÁC HÀM HỖ TRỢ ĐỌC/GHI XUỐNG DATABASE ---
        private string GetConfigValue(System.Collections.Generic.List<CauHinhHeThong> configs, string key)
        {
            return configs.FirstOrDefault(c => c.MaCauHinh == key)?.GiaTriCauHinh ?? "";
        }

        private void SetConfigValue(string  key, string ? value, string ? description)
        {
            var config = _context.CauHinhHeThongs.FirstOrDefault(c => c.MaCauHinh == key);
            if (config == null)
            {
                // Chưa có key này -> Thêm mới
                _context.CauHinhHeThongs.Add(new CauHinhHeThong
                {
                    MaCauHinh =  key,
                    GiaTriCauHinh = value ?? "",
                    MoTa = description
                });
            }
            else
            {
                // Đã có -> Sửa đè giá trị
                config.GiaTriCauHinh = value ?? "";
                config.MoTa = description;
            }
        }
    }
}