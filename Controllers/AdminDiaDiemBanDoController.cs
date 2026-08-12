using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebMTTQ.Models;
using Microsoft.Extensions.Caching.Memory; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG CACHE
using System;

namespace WebMTTQ.Controllers
{
    [KiemTraQuyen(ModuleQuyen.DiaDiemBanDo, "Xem")]
    public class AdminDiaDiemBanDoController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IMemoryCache _cache; // Thêm biến _cache

        // Cập nhật Constructor để Inject IMemoryCache vào
        public AdminDiaDiemBanDoController(DataMTTQContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // ==========================================
        // 1. DANH SÁCH (READ)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var data = await _context.DiaDiemBanDos.AsNoTracking()
                                     .Where(x => x.DaXoa != true)
                                     .OrderByDescending(x => x.IddiaDiem)
                                     .ToListAsync();

            return View("~/Views/Admin/DiaDiemBanDo/Index.cshtml", data);
        }

        // ==========================================
        // 2. FORM THÊM MỚI (GET)
        // ==========================================
        public IActionResult Create()
        {
            return View("~/Views/Admin/DiaDiemBanDo/Create.cshtml");
        }

        // ==========================================
        // 3. XỬ LÝ THÊM MỚI (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiaDiemBanDo model, IFormFile HinhAnhUpload)
        {
            if (ModelState.IsValid)
            {
                // Làm tròn tọa độ về 6 số thập phân chuẩn của Google Maps
                model.ViDo = Math.Round(model.ViDo, 6);
                model.KinhDo = Math.Round(model.KinhDo, 6);

                model.DaXoa = false;

                if (HinhAnhUpload != null && HinhAnhUpload.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await HinhAnhUpload.CopyToAsync(ms);
                        model.HinhAnhThucTe = ms.ToArray();
                    }
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                // ---> QUAN TRỌNG: XÓA CACHE BẢN ĐỒ NGAY SAU KHI THÊM THÀNH CÔNG <---
                _cache.Remove("BanDoData");

                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Thêm địa điểm bản đồ thành công!";

                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/DiaDiemBanDo/Create.cshtml", model);
        }

        // ==========================================
        // 4. XÓA MỀM (POST)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.DiaDiemBanDos.FindAsync(id);
            if (item != null)
            {
                item.DaXoa = true; // Xóa mềm
                await _context.SaveChangesAsync();
                // ---> QUAN TRỌNG: XÓA CACHE BẢN ĐỒ NGAY SAU KHI XÓA THÀNH CÔNG <---
                _cache.Remove("BanDoData");

                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Đã xóa địa điểm thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. FORM CHỈNH SỬA (GET)
        // ==========================================
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.DiaDiemBanDos.FindAsync(id);
            if (item == null) return NotFound();

            return View("~/Views/Admin/DiaDiemBanDo/Edit.cshtml", item);
        }

        // ==========================================
        // 6. XỬ LÝ CHỈNH SỬA (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiaDiemBanDo model, IFormFile HinhAnhUpload)
        {
            if (id != model.IddiaDiem) return NotFound();

            if (ModelState.IsValid)
            {
                // Làm tròn tọa độ
                model.ViDo = Math.Round(model.ViDo, 6);
                model.KinhDo = Math.Round(model.KinhDo, 6);

                var existingItem = await _context.DiaDiemBanDos.AsNoTracking().FirstOrDefaultAsync(x => x.IddiaDiem == id);

                if (existingItem != null)
                {
                    if (HinhAnhUpload != null && HinhAnhUpload.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            await HinhAnhUpload.CopyToAsync(ms);
                            model.HinhAnhThucTe = ms.ToArray();
                        }
                    }
                    else
                    {
                        model.HinhAnhThucTe = existingItem.HinhAnhThucTe;
                    }
                }

                model.DaXoa = false;
                _context.Update(model);
                await _context.SaveChangesAsync();
                // ---> QUAN TRỌNG: XÓA CACHE BẢN ĐỒ NGAY SAU KHI SỬA THÀNH CÔNG <---
                _cache.Remove("BanDoData");

                // THÊM DÒNG NÀY
                TempData["SuccessMessage"] = "Cập nhật địa điểm thành công!";

                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/DiaDiemBanDo/Edit.cshtml", model);
        }
    }
}