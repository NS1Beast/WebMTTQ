using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    // CHIÊU THỨC TRỊ LỖI 404 TẬN GỐC: 
    // Chấp nhận cả 2 loại đường dẫn (có dấu gạch chéo và không có dấu gạch chéo)!
    [Route("AdminQuyCuuTro/[action]/{id?}")]
    [Route("Admin/QuyCuuTro/[action]/{id?}")]
    public class AdminQuyCuuTroController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminQuyCuuTroController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. THÔNG TIN TIẾP NHẬN
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ThongTin()
        {
            ViewData["Title"] = "Tài khoản tiếp nhận Cứu trợ";
            var data = await _context.ThongTinNhanUngHoCuuTros.OrderByDescending(x => x.Id).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/ThongTin.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemThongTin() => View("~/Views/Admin/QuyCuuTro/ThemThongTin.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemThongTin(ThongTinNhanUngHoCuuTro model, IFormFile? QrCodeFile)
        {
            if (QrCodeFile != null && QrCodeFile.Length > 0)
            {
                if (QrCodeFile.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước ảnh vượt quá 2MB!";
                    return View("~/Views/Admin/QuyCuuTro/ThemThongTin.cshtml", model);
                }
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "qrcodes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(QrCodeFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await QrCodeFile.CopyToAsync(fileStream);
                }
                model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName;
            }
            model.TrangThai = true;
            _context.ThongTinNhanUngHoCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm tài khoản thành công!";
            return RedirectToAction("ThongTin");
        }

        [HttpGet]
        public async Task<IActionResult> SuaThongTin(int id)
        {
            var item = await _context.ThongTinNhanUngHoCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaThongTin.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaThongTin(ThongTinNhanUngHoCuuTro model, IFormFile? QrCodeFile)
        {
            if (QrCodeFile != null && QrCodeFile.Length > 0)
            {
                if (QrCodeFile.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước ảnh vượt quá 2MB!";
                    return RedirectToAction("SuaThongTin", new { id = model.Id });
                }
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "qrcodes");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(QrCodeFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await QrCodeFile.CopyToAsync(fileStream);
                }
                model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName;
            }
            _context.ThongTinNhanUngHoCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
            return RedirectToAction("ThongTin");
        }

        [HttpPost]
        public async Task<IActionResult> XoaThongTin(int id)
        {
            var item = await _context.ThongTinNhanUngHoCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.ThongTinNhanUngHoCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa tài khoản!";
            }
            return RedirectToAction("ThongTin");
        }

        // ==========================================
        // 2. SỐ DƯ QUỸ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> SoDu()
        {
            ViewData["Title"] = "Số dư Quỹ Cứu trợ";
            var data = await _context.SoDuQuyCuuTros.OrderByDescending(x => x.NgayCapNhat).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/SoDu.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemSoDu() => View("~/Views/Admin/QuyCuuTro/ThemSoDu.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemSoDu(SoDuQuyCuuTro model)
        {
            model.NgayCapNhat = DateTime.Now;
            model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
            _context.SoDuQuyCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật số dư mới thành công!";
            return RedirectToAction("SoDu");
        }

        [HttpGet]
        public async Task<IActionResult> SuaSoDu(int id)
        {
            var item = await _context.SoDuQuyCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaSoDu.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaSoDu(SoDuQuyCuuTro model)
        {
            model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
            _context.SoDuQuyCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật số dư thành công!";
            return RedirectToAction("SoDu");
        }

        [HttpPost]
        public async Task<IActionResult> XoaSoDu(int id)
        {
            var item = await _context.SoDuQuyCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.SoDuQuyCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa bản ghi số dư!";
            }
            return RedirectToAction("SoDu");
        }

        // ==========================================
        // 3. DANH SÁCH ỦNG HỘ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            ViewData["Title"] = "Danh sách ủng hộ Cứu trợ";
            var data = await _context.DanhSachUngHoCuuTros.OrderByDescending(x => x.NgayUngHo).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/DanhSach.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemDanhSach() => View("~/Views/Admin/QuyCuuTro/ThemDanhSach.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemDanhSach(DanhSachUngHoCuuTro model)
        {
            model.HienThi = true;
            _context.DanhSachUngHoCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm lượt ủng hộ thành công!";
            return RedirectToAction("DanhSach");
        }

        [HttpGet]
        public async Task<IActionResult> SuaDanhSach(int id)
        {
            var item = await _context.DanhSachUngHoCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaDanhSach.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaDanhSach(DanhSachUngHoCuuTro model)
        {
            _context.DanhSachUngHoCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật lượt ủng hộ thành công!";
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public async Task<IActionResult> XoaDanhSach(int id)
        {
            var item = await _context.DanhSachUngHoCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.DanhSachUngHoCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa lượt ủng hộ!";
            }
            return RedirectToAction("DanhSach");
        }

        // ==========================================
        // 4. KẾT QUẢ HOẠT ĐỘNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> KetQua()
        {
            ViewData["Title"] = "Hoạt động Cứu trợ";
            var data = await _context.KetQuaHoatDongCuuTros.OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang).ToListAsync();
            return View("~/Views/Admin/QuyCuuTro/KetQua.cshtml", data);
        }

        [HttpGet]
        public IActionResult ThemKetQua() => View("~/Views/Admin/QuyCuuTro/ThemKetQua.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemKetQua(KetQuaHoatDongCuuTro model)
        {
            model.TrangThai = true;
            _context.KetQuaHoatDongCuuTros.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm hoạt động thành công!";
            return RedirectToAction("KetQua");
        }

        [HttpGet]
        public async Task<IActionResult> SuaKetQua(int id)
        {
            var item = await _context.KetQuaHoatDongCuuTros.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyCuuTro/SuaKetQua.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaKetQua(KetQuaHoatDongCuuTro model)
        {
            _context.KetQuaHoatDongCuuTros.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hoạt động thành công!";
            return RedirectToAction("KetQua");
        }

        [HttpPost]
        public async Task<IActionResult> XoaKetQua(int id)
        {
            var item = await _context.KetQuaHoatDongCuuTros.FindAsync(id);
            if (item != null)
            {
                _context.KetQuaHoatDongCuuTros.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa hoạt động!";
            }
            return RedirectToAction("KetQua");
        }
    }
}