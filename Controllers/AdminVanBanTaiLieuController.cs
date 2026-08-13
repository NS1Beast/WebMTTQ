using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    // Bọc Route để chống lỗi 404
    [Route("AdminVanBanTaiLieu/[action]/{id?}")]
    public class AdminVanBanTaiLieuController : Controller
    {
        private readonly DataMTTQContext _context;

        public AdminVanBanTaiLieuController(DataMTTQContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Văn bản & Tài liệu";
            var data = await _context.VanBanTaiLieus
                .Include(v => v.IdchuyenMucNavigation)
                .Where(x => x.DaXoa != true)
                .OrderByDescending(x => x.NgayBanHanh)
                .ThenByDescending(x => x.IdvanBan)
                .ToListAsync();

            return View("~/Views/Admin/VanBanTaiLieu/Index.cshtml", data);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm Văn bản mới";
            // ĐÃ SỬA: Dùng đúng cột "IdchuyenMuc"
            ViewBag.ChuyenMucs = new SelectList(_context.ChuyenMucs.Where(c => c.DaXoa != true), "IdchuyenMuc", "TenChuyenMuc");
            return View("~/Views/Admin/VanBanTaiLieu/Create.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(VanBanTaiLieu model, IFormFile? FileUpload)
        {
            if (FileUpload != null && FileUpload.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await FileUpload.CopyToAsync(ms);
                    model.TepDinhKem = ms.ToArray();
                }
                model.LoaiTep = Path.GetExtension(FileUpload.FileName);
                model.DungLuong = FileUpload.Length;
            }

            model.DaXoa = false;
            _context.VanBanTaiLieus.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm văn bản thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.VanBanTaiLieus.FindAsync(id);
            if (item == null || item.DaXoa == true) return NotFound();

            ViewData["Title"] = "Cập nhật Văn bản";
            // ĐÃ SỬA: Dùng đúng cột "IdchuyenMuc"
            ViewBag.ChuyenMucs = new SelectList(_context.ChuyenMucs.Where(c => c.DaXoa != true), "IdchuyenMuc", "TenChuyenMuc", item.IdchuyenMuc);
            return View("~/Views/Admin/VanBanTaiLieu/Edit.cshtml", item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VanBanTaiLieu model, IFormFile? FileUpload)
        {
            var existingItem = await _context.VanBanTaiLieus.AsNoTracking().FirstOrDefaultAsync(x => x.IdvanBan == model.IdvanBan);
            if (existingItem == null) return NotFound();

            if (FileUpload != null && FileUpload.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await FileUpload.CopyToAsync(ms);
                    model.TepDinhKem = ms.ToArray();
                }
                model.LoaiTep = Path.GetExtension(FileUpload.FileName);
                model.DungLuong = FileUpload.Length;
            }
            else
            {
                model.TepDinhKem = existingItem.TepDinhKem;
                model.LoaiTep = existingItem.LoaiTep;
                model.DungLuong = existingItem.DungLuong;
            }

            model.DaXoa = existingItem.DaXoa;
            _context.VanBanTaiLieus.Update(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật văn bản thành công!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.VanBanTaiLieus.FindAsync(id);
            if (item != null)
            {
                item.DaXoa = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa văn bản thành công!";
            }
            return RedirectToAction("Index");
        }
    }
}