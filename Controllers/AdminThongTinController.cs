using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using Microsoft.AspNetCore.Hosting; // Bắt buộc thêm để lấy đường dẫn wwwroot
using Microsoft.AspNetCore.Http; // Bắt buộc thêm để dùng IFormFile
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebMTTQ.Controllers
{
    [Route("admin/thongtinungho")]
    public class AdminThongTinController : BaseAdminController
    {
        private readonly DataMTTQContext _context;
        private readonly IWebHostEnvironment _env; // Khai báo biến môi trường

        // Tiêm IWebHostEnvironment vào constructor
        public AdminThongTinController(DataMTTQContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var list = await _context.ThongTinNhanUngHos.AsNoTracking().ToListAsync();
            return View("~/Views/Admin/ThongTinUngHo/Index.cshtml", list);
        }

        [Route("Create")]
        public IActionResult Create()
        {
            return View("~/Views/Admin/ThongTinUngHo/Create.cshtml");
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        // Thêm tham số IFormFile FileQr để nhận ảnh từ thẻ <input type="file" name="FileQr">
        public async Task<IActionResult> Create(ThongTinNhanUngHo model, IFormFile FileQr)
        {
            if (ModelState.IsValid)
            {
                // Xử lý lưu file ảnh nếu có tải lên
                if (FileQr != null && FileQr.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "qrcodes");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileQr.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FileQr.CopyToAsync(fileStream);
                    }

                    // Gán đường dẫn vào Model để lưu vào SQL
                    model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/ThongTinUngHo/Create.cshtml", model);
        }

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
        // Thêm tham số IFormFile FileQr để xử lý cập nhật ảnh
        public async Task<IActionResult> Edit(int id, ThongTinNhanUngHo model, IFormFile FileQr)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu người dùng chọn tải ảnh QR mới lên
                    if (FileQr != null && FileQr.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "qrcodes");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(FileQr.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await FileQr.CopyToAsync(fileStream);
                        }

                        model.QrCodeUrl = "/uploads/qrcodes/" + uniqueFileName; // Đổi sang ảnh mới
                    }
                    // Ghi chú: Nếu không up ảnh mới, model.QrCodeUrl sẽ tự động lấy giá trị cũ từ thẻ <input type="hidden"> ở View

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