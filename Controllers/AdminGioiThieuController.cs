using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin/gioithieu")]
    public class AdminGioiThieuController : BaseAdminController
    {
        private readonly DataMTTQContext _context;

        public AdminGioiThieuController(DataMTTQContext context)
        {
            _context = context;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.GioiThieuChungs.OrderByDescending(x => x.Id).ToListAsync();
            return View("~/Views/Admin/GioiThieu/Index.cshtml", list);
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create() => View("~/Views/Admin/GioiThieu/Create.cshtml");

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GioiThieuChung model)
        {
            if (ModelState.IsValid)
            {
                // Tắt các trạng thái hiển thị của những cái cũ (Chỉ giữ 1 cái hiển thị)
                if (model.TrangThai == true)
                {
                    var oldActives = await _context.GioiThieuChungs.Where(x => x.TrangThai == true).ToListAsync();
                    oldActives.ForEach(x => x.TrangThai = false);
                }

                _context.GioiThieuChungs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm phần giới thiệu thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/GioiThieu/Create.cshtml", model);
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.GioiThieuChungs.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/GioiThieu/Edit.cshtml", item);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GioiThieuChung model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (model.TrangThai == true)
                {
                    var oldActives = await _context.GioiThieuChungs.Where(x => x.TrangThai == true && x.Id != id).ToListAsync();
                    oldActives.ForEach(x => x.TrangThai = false);
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/GioiThieu/Edit.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GioiThieuChungs.FindAsync(id);
            if (item != null)
            {
                _context.GioiThieuChungs.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Toggle/{id}")]
        public async Task<IActionResult> Toggle(int id)
        {
            var item = await _context.GioiThieuChungs.FindAsync(id);
            if (item != null)
            {
                var oldActives = await _context.GioiThieuChungs.Where(x => x.TrangThai == true && x.Id != id).ToListAsync();
                oldActives.ForEach(x => x.TrangThai = false);

                item.TrangThai = !item.TrangThai;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thay đổi hiển thị!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}