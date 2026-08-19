using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin/soduquy")]
    [KiemTraQuyen(ModuleQuyen.SoDuQuy, "Xem")]
    public class AdminSoDuQuyController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminSoDuQuyController(DataMTTQContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.SoDuQues.AsNoTracking()
                .Where(x => x.LoaiQuy == "NguoiNgheo")
                .OrderByDescending(x => x.NgayCapNhat)
                .ToListAsync();
            return View("~/Views/Admin/SoDuQuy/Index.cshtml", list);
        }

        [Route("Create")]
        [KiemTraQuyen(ModuleQuyen.SoDuQuy, "Them")]
        public IActionResult Create() => View("~/Views/Admin/SoDuQuy/Create.cshtml");

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [KiemTraQuyen(ModuleQuyen.SoDuQuy, "Them")]
        public async Task<IActionResult> Create(SoDuQuy model)
        {
            if (ModelState.IsValid)
            {
                model.NgayCapNhat = DateTime.Now;
                model.LoaiQuy = "NguoiNgheo";
                model.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);
                _context.SoDuQues.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm số dư quỹ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/SoDuQuy/Create.cshtml", model);
        }

        [Route("Edit/{id}")]
        [KiemTraQuyen(ModuleQuyen.SoDuQuy, "Sua")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.SoDuQues.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/SoDuQuy/Edit.cshtml", item);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [KiemTraQuyen(ModuleQuyen.SoDuQuy, "Sua")]
        public async Task<IActionResult> Edit(int id, SoDuQuy model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var existingItem = await _context.SoDuQues.FindAsync(id);
                if (existingItem == null) return NotFound();

                existingItem.NgayCapNhat = DateTime.Now;
                existingItem.TienMat = model.TienMat;
                existingItem.TienGuiNganHang = model.TienGuiNganHang;
                existingItem.TongTonQuy = (model.TienMat ?? 0) + (model.TienGuiNganHang ?? 0);

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật số dư quỹ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/SoDuQuy/Edit.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        [KiemTraQuyen(ModuleQuyen.SoDuQuy, "Xoa")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SoDuQues.FindAsync(id);
            if (item != null && item.LoaiQuy == "NguoiNgheo")
            {
                _context.SoDuQues.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa số dư quỹ thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}