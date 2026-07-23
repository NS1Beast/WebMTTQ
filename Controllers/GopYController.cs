using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;

namespace WebMTTQ.Controllers
{
    public class GopYController : Controller
    {
        private readonly DataMTTQContext _context;

        public GopYController(DataMTTQContext context)
        {
            _context = context;
        }

        // 1. Hàm hiển thị trang form
        [HttpGet]
        public IActionResult Index()
        {
            // SỬA Ở ĐÂY: Trỏ đúng đường dẫn file View của bạn
            // Ví dụ bên dưới giả sử bạn để ở Views/GopY/Index.cshtml
            return View("~/Views/GopY/Index.cshtml");
        }

        // 2. Hàm xử lý khi người dùng bấm nút "Gửi góp ý"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiGopY(HopThuGopY model, IFormFile tepDinhKem)
        {
            if (ModelState.IsValid)
            {
                model.NgayGui = DateTime.Now;
                model.TrangThai = "Chưa xử lý";
                model.DaXoa = false;

                if (tepDinhKem != null && tepDinhKem.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await tepDinhKem.CopyToAsync(memoryStream);
                        model.TepMinhChung = memoryStream.ToArray();
                    }
                }

                _context.HopThuGopies.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Gửi góp ý thành công. Trân trọng cảm ơn ý kiến của Quý vị!";
                return RedirectToAction("Index");
            }

            // SỬA Ở ĐÂY: Nếu dữ liệu nhập lỗi (ví dụ quên nhập tên), 
            // phải trả lại đúng đường dẫn View để hiển thị thông báo lỗi
            return View("~/Views/GopY/Index.cshtml", model);
        }
    }
}