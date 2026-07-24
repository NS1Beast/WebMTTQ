using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    // [Authorize(Roles = "Admin")] // Bỏ comment nếu hệ thống có phân quyền
    public class AdminNguoiDanCanTroGiupController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminNguoiDanCanTroGiupController(DataMTTQContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH
        public async Task<IActionResult> Index()
        {
            var data = await _context.NguoiDanCanTroGiups
                                     .Where(x => x.DaXoa != true)
                                     .OrderByDescending(x => x.NgayGui)
                                     .ToListAsync();
            return View("~/Views/Admin/NguoiDanCanTroGiup/Index.cshtml", data);
        }

        // 2. XEM VÀ CẬP NHẬT TRẠNG THÁI (EDIT)
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.NguoiDanCanTroGiups.FindAsync(id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/NguoiDanCanTroGiup/Edit.cshtml", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NguoiDanCanTroGiup model)
        {
            if (id != model.Id) return NotFound();

            var existingItem = await _context.NguoiDanCanTroGiups.FindAsync(id);
            if (existingItem != null)
            {
                // Chỉ cho phép admin cập nhật trạng thái
                existingItem.TrangThai = model.TrangThai;
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Cập nhật trạng thái xử lý thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/NguoiDanCanTroGiup/Edit.cshtml", model);
        }

        // 3. XÓA MỀM
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.NguoiDanCanTroGiups.FindAsync(id);
            if (item != null)
            {
                item.DaXoa = true;
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Đã xóa yêu cầu trợ giúp thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}