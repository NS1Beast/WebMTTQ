using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; // Thư viện lấy đường dẫn wwwroot
using Microsoft.AspNetCore.Http; // Thư viện xử lý File
using System.IO; // Thư viện xử lý thư mục
using System;

namespace WebMTTQ.Controllers
{
    public class AdminBannerController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        // Tiêm IWebHostEnvironment vào để lấy thư mục lưu ảnh
        public AdminBannerController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. DANH SÁCH BANNER
        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners.OrderBy(b => b.ThuTu).ToListAsync();
            return View("~/Views/Admin/Banner/Index.cshtml", banners);
        }

        // 2. HIỂN THỊ FORM THÊM MỚI
        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/Banner/Create.cshtml");
        }

        // 3. XỬ LÝ LƯU BANNER MỚI CÓ ẢNH UPLOAD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile FileAnh)
        {
            ModelState.Remove("HinhAnh"); // Bỏ qua validate thuộc tính text HinhAnh mặc định

            if (ModelState.IsValid)
            {
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    // Kiểm tra dung lượng < 3MB (3 * 1024 * 1024 bytes)
                    if (FileAnh.Length > 3 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 3MB.");
                        return View("~/Views/Admin/Banner/Create.cshtml", banner);
                    }

                    // Tạo thư mục nếu chưa có
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // Tạo tên file duy nhất tránh trùng lặp
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileAnh.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file vào thư mục wwwroot/uploads/banners
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileAnh.CopyToAsync(fileStream);
                    }

                    // Gán đường dẫn vào DB
                    banner.HinhAnh = "/uploads/banners/" + uniqueFileName;
                }
                else
                {
                    ModelState.AddModelError("HinhAnh", "Vui lòng chọn ảnh Banner.");
                    return View("~/Views/Admin/Banner/Create.cshtml", banner);
                }

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm Banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/Banner/Create.cshtml", banner);
        }

        // 4. HIỂN THỊ FORM SỬA BANNER
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            return View("~/Views/Admin/Banner/Edit.cshtml", banner);
        }

        // 5. XỬ LÝ CẬP NHẬT BANNER
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile FileAnh)
        {
            if (id != banner.IdBanner) return NotFound();

            ModelState.Remove("HinhAnh"); // Bỏ qua validate thuộc tính text HinhAnh mặc định

            if (ModelState.IsValid)
            {
                // Lấy banner cũ từ DB để giữ nguyên ảnh cũ nếu không up ảnh mới
                var existingBanner = await _context.Banners.FindAsync(id);
                if (existingBanner == null) return NotFound();

                // Cập nhật các thông tin văn bản
                existingBanner.TieuDe = banner.TieuDe;
                existingBanner.LienKet = banner.LienKet;
                existingBanner.ThuTu = banner.ThuTu;
                existingBanner.TrangThai = banner.TrangThai;

                // Nếu người dùng có chọn ảnh mới
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    if (FileAnh.Length > 3 * 1024 * 1024)
                    {
                        ModelState.AddModelError("HinhAnh", "Kích thước ảnh không được vượt quá 3MB.");
                        return View("~/Views/Admin/Banner/Edit.cshtml", existingBanner);
                    }

                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "banners");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileAnh.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileAnh.CopyToAsync(fileStream);
                    }

                    existingBanner.HinhAnh = "/uploads/banners/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật Banner thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/Banner/Edit.cshtml", banner);
        }

        // 6. XÓA BANNER
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Banner thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}