using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    [Route("admin/doanthe")]
    public class AdminDoanTheToChucController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminDoanTheToChucController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.DoanTheToChucs
                .Where(x => x.DaXoa != true)
                .OrderBy(x => x.CapDo).ThenBy(x => x.ThuTu)
                .ToListAsync();
            return View("~/Views/Admin/DoanTheToChuc/Index.cshtml", list);
        }

        [Route("Create")]
        [HttpGet]
        public IActionResult Create() => View("~/Views/Admin/DoanTheToChuc/Create.cshtml");

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoanTheToChuc model, IFormFile? FileAnh)
        {
            if (ModelState.IsValid)
            {
                if (FileAnh != null && FileAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "doanthe");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileAnh.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileAnh.CopyToAsync(fileStream);
                    }
                    model.HinhAnh = "/uploads/doanthe/" + uniqueFileName;
                }
                else { model.HinhAnh = "/images/LogoGioiThieu/logoMTTQ.png"; }

                model.DaXoa = false;
                _context.DoanTheToChucs.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm thành viên tổ chức thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DoanTheToChuc/Create.cshtml", model);
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.DoanTheToChucs.FindAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/DoanTheToChuc/Edit.cshtml", item);
        }

        [Route("Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoanTheToChuc model, IFormFile? FileAnh)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.DoanTheToChucs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (existing == null) return NotFound();

                if (FileAnh != null && FileAnh.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "doanthe");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileAnh.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileAnh.CopyToAsync(fileStream);
                    }
                    model.HinhAnh = "/uploads/doanthe/" + uniqueFileName;
                }
                else { model.HinhAnh = existing.HinhAnh; }

                model.DaXoa = existing.DaXoa;
                _context.Update(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DoanTheToChuc/Edit.cshtml", model);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.DoanTheToChucs.FindAsync(id);
            if (item != null)
            {
                item.DaXoa = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa khỏi sơ đồ tổ chức!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}