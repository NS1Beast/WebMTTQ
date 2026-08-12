using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System.Threading.Tasks;
using System.Linq;

namespace WebMTTQ.Controllers
{
    [Route("admin/danhsachungho")]
    [KiemTraQuyen(ModuleQuyen.DanhSachUngHo, "Xem")]
    public class AdminDanhSachUngHoController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminDanhSachUngHoController(DataMTTQContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            // Sắp xếp ngày mới nhất lên đầu
            var list = await _context.DanhSachUngHos.AsNoTracking().OrderByDescending(x => x.NgayUngHo).ToListAsync();
            return View("~/Views/Admin/DanhSachUngHo/Index.cshtml", list);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/DanhSachUngHo/Create.cshtml");
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhSachUngHo model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY TRƯỚC KHI RETURN
                TempData["SuccessMessage"] = "Thêm danh sách ủng hộ thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DanhSachUngHo/Create.cshtml", model);
        }
        // --- BẮT ĐẦU PHẦN CODE SỬA ---
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Tìm record trong database theo Id
            var item = await _context.DanhSachUngHos.FindAsync(id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/DanhSachUngHo/Edit.cshtml", item);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhSachUngHo model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    // THÊM DÒNG NÀY SAU KHI LƯU THÀNH CÔNG
                    TempData["SuccessMessage"] = "Cập nhật danh sách ủng hộ thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DanhSachUngHos.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DanhSachUngHo/Edit.cshtml", model);
        }
        // --- KẾT THÚC PHẦN CODE SỬA ---
        // Action Xóa
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.DanhSachUngHos.FindAsync(id);
            if (item != null)
            {
                _context.DanhSachUngHos.Remove(item);
                await _context.SaveChangesAsync();
                // THÊM DÒNG NÀY VÀO TRONG KHỐI LỆNH IF
                TempData["SuccessMessage"] = "Đã xóa bản ghi thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}