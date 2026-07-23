using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models; // Sửa lại namespace Models theo dự án của bạn
using System.Linq;
using System.Threading.Tasks;

namespace WebMTTQ.Controllers
{
    public class AdminGopYController : Controller
    {
        private readonly DataMTTQContext _context; // Đổi WebMTTQContext thành tên DbContext của bạn

        public AdminGopYController(DataMTTQContext context)
        {
            _context = context;
        }

        // 1. TRANG DANH SÁCH GÓP Ý
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách góp ý chưa bị xóa, sắp xếp mới nhất lên đầu
            var danhSach = await _context.HopThuGopies
                                .Where(g => g.DaXoa == false || g.DaXoa == null)
                                .OrderByDescending(g => g.NgayGui)
                                .ToListAsync();

            // CHỈ ĐỊNH ĐƯỜNG DẪN VIEW CỤ THỂ THEO CẤU TRÚC CỦA BẠN
            return View("~/Views/Admin/GopY/Index.cshtml", danhSach);
        }

        // 2. TRANG XEM CHI TIẾT & XỬ LÝ
        public async Task<IActionResult> Details(int id)
        {
            var gopy = await _context.HopThuGopies.FindAsync(id);
            if (gopy == null)
            {
                return NotFound();
            }

            // CHỈ ĐỊNH ĐƯỜNG DẪN VIEW CỤ THỂ THEO CẤU TRÚC CỦA BẠN
            return View("~/Views/Admin/GopY/Details.cshtml", gopy);
        }

        // 3. HÀM CẬP NHẬT TRẠNG THÁI XỬ LÝ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuLy(int id, string TrangThai, string NoiDungPhanHoi)
        {
            var gopy = await _context.HopThuGopies.FindAsync(id);
            if (gopy == null)
            {
                return NotFound();
            }

            // Cập nhật trạng thái và phản hồi
            gopy.TrangThai = TrangThai;
            gopy.NoiDungPhanHoi = NoiDungPhanHoi;

            _context.Update(gopy);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật trạng thái xử lý thành công!";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // 4. HÀM XÓA (ẨN) GÓP Ý
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var gopy = await _context.HopThuGopies.FindAsync(id);
            if (gopy != null)
            {
                gopy.DaXoa = true; // Đánh dấu xóa mềm
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa góp ý khỏi danh sách!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}