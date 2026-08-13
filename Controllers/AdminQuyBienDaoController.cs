using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
namespace WebMTTQ.Controllers
{
    public class AdminQuyBienDaoController : Controller
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminQuyBienDaoController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==========================================
        // 1. THÔNG TIN TIẾP NHẬN BẢN ĐẢO
        // ==========================================
        public async Task<IActionResult> ThongTin()
        {
            ViewData["Title"] = "Tài khoản tiếp nhận Biển Đảo";
            var data = await _context.ThongTinNhanUngHoBienDaos.OrderByDescending(x => x.Id).ToListAsync();
            return View("~/Views/Admin/QuyViBienDao/ThongTin.cshtml", data);
        }

        [HttpGet] public IActionResult ThemThongTin() => View("~/Views/Admin/QuyViBienDao/ThemThongTin.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemThongTin(ThongTinNhanUngHoBienDao model, IFormFile? QrCodeFile)
        {
            // Xử lý Upload ảnh QR
            if (QrCodeFile != null && QrCodeFile.Length > 0)
            {
                if (QrCodeFile.Length > 2 * 1024 * 1024) // Lớn hơn 2MB
                {
                    TempData["ErrorMessage"] = "Kích thước ảnh vượt quá 2MB!";
                    return View("~/Views/Admin/QuyViBienDao/ThemThongTin.cshtml", model);
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
            _context.ThongTinNhanUngHoBienDaos.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm tài khoản thành công!";
            return RedirectToAction("ThongTin");
        }

        [HttpGet]
        public async Task<IActionResult> SuaThongTin(int id)
        {
            var item = await _context.ThongTinNhanUngHoBienDaos.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyViBienDao/SuaThongTin.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaThongTin(ThongTinNhanUngHoBienDao model, IFormFile? QrCodeFile)
        {
            // Xử lý Upload ảnh QR mới (nếu có)
            if (QrCodeFile != null && QrCodeFile.Length > 0)
            {
                if (QrCodeFile.Length > 2 * 1024 * 1024) // Lớn hơn 2MB
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
                // Ghi đè URL ảnh mới vào model
                model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName;
            }

            _context.ThongTinNhanUngHoBienDaos.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
            return RedirectToAction("ThongTin");
        }

        [HttpPost]
        public async Task<IActionResult> XoaThongTin(int id)
        {
            var item = await _context.ThongTinNhanUngHoBienDaos.FindAsync(id);
            if (item != null) { _context.ThongTinNhanUngHoBienDaos.Remove(item); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Đã xóa tài khoản!"; }
            return RedirectToAction("ThongTin");
        }


        // ==========================================
        // 2. SỐ DƯ QUỸ BIỂN ĐẢO
        // ==========================================
        public async Task<IActionResult> SoDu()
        {
            ViewData["Title"] = "Số dư Quỹ Biển Đảo";
            var data = await _context.SoDuQuyBienDaos.OrderByDescending(x => x.NgayCapNhat).ToListAsync();
            return View("~/Views/Admin/QuyViBienDao/SoDu.cshtml", data);
        }

        [HttpGet] public IActionResult ThemSoDu() => View("~/Views/Admin/QuyViBienDao/ThemSoDu.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemSoDu(SoDuQuyBienDao model)
        {
            model.NgayCapNhat = DateTime.Now;
            model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
            _context.SoDuQuyBienDaos.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật số dư mới thành công!";
            return RedirectToAction("SoDu");
        }

        [HttpGet]
        public async Task<IActionResult> SuaSoDu(int id)
        {
            var item = await _context.SoDuQuyBienDaos.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyViBienDao/SuaSoDu.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaSoDu(SoDuQuyBienDao model)
        {
            model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
            _context.SoDuQuyBienDaos.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật số dư thành công!";
            return RedirectToAction("SoDu");
        }

        [HttpPost]
        public async Task<IActionResult> XoaSoDu(int id)
        {
            var item = await _context.SoDuQuyBienDaos.FindAsync(id);
            if (item != null) { _context.SoDuQuyBienDaos.Remove(item); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Đã xóa số dư!"; }
            return RedirectToAction("SoDu");
        }


        // ==========================================
        // 3. DANH SÁCH ỦNG HỘ BIỂN ĐẢO
        // ==========================================
        public async Task<IActionResult> DanhSach()
        {
            ViewData["Title"] = "Danh sách ủng hộ Biển Đảo";
            var data = await _context.DanhSachUngHoBienDaos.OrderByDescending(x => x.NgayUngHo).ToListAsync();
            return View("~/Views/Admin/QuyViBienDao/DanhSach.cshtml", data);
        }

        [HttpGet] public IActionResult ThemDanhSach() => View("~/Views/Admin/QuyViBienDao/ThemDanhSach.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemDanhSach(DanhSachUngHoBienDao model)
        {
            model.HienThi = true;
            _context.DanhSachUngHoBienDaos.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm lượt ủng hộ thành công!";
            return RedirectToAction("DanhSach");
        }

        [HttpGet]
        public async Task<IActionResult> SuaDanhSach(int id)
        {
            var item = await _context.DanhSachUngHoBienDaos.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyViBienDao/SuaDanhSach.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaDanhSach(DanhSachUngHoBienDao model)
        {
            _context.DanhSachUngHoBienDaos.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("DanhSach");
        }

        [HttpPost]
        public async Task<IActionResult> XoaDanhSach(int id)
        {
            var item = await _context.DanhSachUngHoBienDaos.FindAsync(id);
            if (item != null) { _context.DanhSachUngHoBienDaos.Remove(item); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Đã xóa lượt ủng hộ!"; }
            return RedirectToAction("DanhSach");
        }


        // ==========================================
        // 4. KẾT QUẢ HOẠT ĐỘNG BIỂN ĐẢO
        // ==========================================
        public async Task<IActionResult> KetQua()
        {
            ViewData["Title"] = "Hoạt động hướng về Biển Đảo";
            var data = await _context.KetQuaHoatDongBienDaos.OrderByDescending(x => x.Nam).ThenByDescending(x => x.Thang).ToListAsync();
            return View("~/Views/Admin/QuyViBienDao/KetQua.cshtml", data);
        }

        [HttpGet] public IActionResult ThemKetQua() => View("~/Views/Admin/QuyViBienDao/ThemKetQua.cshtml");

        [HttpPost]
        public async Task<IActionResult> ThemKetQua(KetQuaHoatDongBienDao model)
        {
            model.TrangThai = true;
            _context.KetQuaHoatDongBienDaos.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thêm hoạt động thành công!";
            return RedirectToAction("KetQua");
        }

        [HttpGet]
        public async Task<IActionResult> SuaKetQua(int id)
        {
            var item = await _context.KetQuaHoatDongBienDaos.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/QuyViBienDao/SuaKetQua.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> SuaKetQua(KetQuaHoatDongBienDao model)
        {
            _context.KetQuaHoatDongBienDaos.Update(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hoạt động thành công!";
            return RedirectToAction("KetQua");
        }

        [HttpPost]
        public async Task<IActionResult> XoaKetQua(int id)
        {
            var item = await _context.KetQuaHoatDongBienDaos.FindAsync(id);
            if (item != null) { _context.KetQuaHoatDongBienDaos.Remove(item); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Đã xóa hoạt động!"; }
            return RedirectToAction("KetQua");
        }
    }
}