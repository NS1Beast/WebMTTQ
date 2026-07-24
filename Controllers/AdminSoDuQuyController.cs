using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin/soduquy")]
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
            // Sắp xếp ngày mới nhất lên đầu
            // Thêm AsNoTracking
            var list = await _context.SoDuQuyViNguoiNgheos.AsNoTracking().OrderByDescending(x => x.NgayCapNhat).ToListAsync();
            return View("~/Views/Admin/SoDuQuy/Index.cshtml", list);
        }

        [Route("Create")]
        public IActionResult Create() => View("~/Views/Admin/SoDuQuy/Create.cshtml");

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SoDuQuyViNguoiNgheo model)
        {
            if (ModelState.IsValid)
            {
                // Tự động gán ngày hiện tại
                model.NgayCapNhat = DateTime.Now;
                _context.SoDuQuyViNguoiNgheos.Add(model);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Thêm số dư quỹ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/SoDuQuy/Create.cshtml", model);
        }

        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.SoDuQuyViNguoiNgheos.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/SoDuQuy/Edit.cshtml", item);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SoDuQuyViNguoiNgheo model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                // Tự động cập nhật ngày hiện tại khi sửa
                model.NgayCapNhat = DateTime.Now;
                _context.Update(model);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Cập nhật số dư quỹ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/SoDuQuy/Edit.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.SoDuQuyViNguoiNgheos.FindAsync(id);
            if (item != null)
            {
                _context.SoDuQuyViNguoiNgheos.Remove(item);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY TRONG KHỐI IF
                TempData["SuccessMessage"] = "Đã xóa số dư quỹ thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}