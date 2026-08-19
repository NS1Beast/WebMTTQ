using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;
using WebMTTQ.Services;

namespace WebMTTQ.Controllers
{
    [KiemTraQuyen(ModuleQuyen.NguoiDanCanTroGiup)]
    public class AdminNguoiDanCanTroGiupController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminNguoiDanCanTroGiupController(DataMTTQContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH
        [KiemTraQuyen(ModuleQuyen.NguoiDanCanTroGiup, "Xem")]
        public async Task<IActionResult> Index()
        {
            var data = await _context.NguoiDanCanTroGiups
                                     .OrderByDescending(x => x.NgayGui)
                                     .AsNoTracking()
                                     .ToListAsync();
            return View("~/Views/Admin/NguoiDanCanTroGiup/Index.cshtml", data);
        }

        // 2. XEM VÀ CẬP NHẬT TRẠNG THÁI (EDIT)
        [KiemTraQuyen(ModuleQuyen.NguoiDanCanTroGiup, "Xem")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.NguoiDanCanTroGiups.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/NguoiDanCanTroGiup/Edit.cshtml", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [KiemTraQuyen(ModuleQuyen.NguoiDanCanTroGiup, "Sua")]
        public async Task<IActionResult> Edit(int id, NguoiDanCanTroGiup model)
        {
            if (id != model.Id) return NotFound();

            var existingItem = await _context.NguoiDanCanTroGiups.FindAsync(id);
            if (existingItem == null) return NotFound();

            // Validate trạng thái hợp lệ
            var trangThaiHopLe = new[] { "Chưa xử lý", "Đang xử lý", "Đã xử lý", "Từ chối" };
            if (string.IsNullOrWhiteSpace(model.TrangThai) || !trangThaiHopLe.Contains(model.TrangThai))
            {
                ModelState.AddModelError("TrangThai", "Trạng thái không hợp lệ.");
                return View("~/Views/Admin/NguoiDanCanTroGiup/Edit.cshtml", model);
            }

            // Chỉ cho phép admin cập nhật trạng thái (bảo vệ chống mass assignment)
            existingItem.TrangThai = model.TrangThai;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái xử lý thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 3. XÓA CỨNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        [KiemTraQuyen(ModuleQuyen.NguoiDanCanTroGiup, "Xoa")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.NguoiDanCanTroGiups.FindAsync(id);
            if (item != null)
            {
                _context.NguoiDanCanTroGiups.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa yêu cầu trợ giúp thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}