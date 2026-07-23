using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    // 1. Ép hệ thống nhận URL này
    [Route("admin/thongtinungho")]
    public class AdminThongTinController : Controller
    {
        private readonly DataMTTQContext _context;

        public AdminThongTinController(DataMTTQContext context)
        {
            _context = context;
        }

        // 2. Định tuyến cho trang danh sách
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.ThongTinNhanUngHos.AsNoTracking().ToListAsync();
            return View("~/Views/Admin/ThongTinUngHo/Index.cshtml", list);
        }

        // 3. Định tuyến cho trang Thêm mới
        [Route("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/ThongTinUngHo/Create.cshtml");
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThongTinNhanUngHo model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/ThongTinUngHo/Create.cshtml", model);
        }

        // 4. Định tuyến cho trang Sửa
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.ThongTinNhanUngHos.FindAsync(id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/ThongTinUngHo/Edit.cshtml", item);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThongTinNhanUngHo model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ThongTinNhanUngHos.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/ThongTinUngHo/Edit.cshtml", model);
        }

        // 5. Định tuyến cho Xóa
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ThongTinNhanUngHos.FindAsync(id);
            if (item != null)
            {
                _context.ThongTinNhanUngHos.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}