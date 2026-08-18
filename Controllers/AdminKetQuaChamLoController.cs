using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin/ketquachamlo")]
    [KiemTraQuyen(ModuleQuyen.KetQuaChamLo, "Xem")]
    public class AdminKetQuaChamLoController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminKetQuaChamLoController(DataMTTQContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            // Thêm AsNoTracking
            var list = await _context.KetQuaChamLos.AsNoTracking().OrderByDescending(x => x.Thang).ThenByDescending(x => x.Id).ToListAsync();
            return View("~/Views/Admin/KetQuaChamLo/Index.cshtml", list);
        }

        [Route("Create")]
        [KiemTraQuyen(ModuleQuyen.KetQuaChamLo, "Them")]
        public IActionResult Create() => View("~/Views/Admin/KetQuaChamLo/Create.cshtml");

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [KiemTraQuyen(ModuleQuyen.KetQuaChamLo, "Them")]
        public async Task<IActionResult> Create(KetQuaChamLo model)
        {
            if (ModelState.IsValid)
            {
                model.NgayCapNhat = DateTime.Now;
                _context.KetQuaChamLos.Add(model);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY 
                TempData["SuccessMessage"] = "Thêm kết quả chăm lo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/KetQuaChamLo/Create.cshtml", model);
        }

        [Route("Edit/{id}")]
        [KiemTraQuyen(ModuleQuyen.KetQuaChamLo, "Sua")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.KetQuaChamLos.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/KetQuaChamLo/Edit.cshtml", item);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [KiemTraQuyen(ModuleQuyen.KetQuaChamLo, "Sua")]
        public async Task<IActionResult> Edit(int id, KetQuaChamLo model)
        {
            if (id != model.Id) return NotFound();
            if (ModelState.IsValid)
            {
                model.NgayCapNhat = DateTime.Now;
                _context.Update(model);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Cập nhật kết quả chăm lo thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/KetQuaChamLo/Edit.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        [KiemTraQuyen(ModuleQuyen.KetQuaChamLo, "Xoa")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.KetQuaChamLos.FindAsync(id);
            if (item != null)
            {
                _context.KetQuaChamLos.Remove(item);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY TRONG KHỐI IF
                TempData["SuccessMessage"] = "Đã xóa kết quả chăm lo thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}