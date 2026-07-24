using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq; // Thư viện cần thiết để dùng các hàm truy vấn như Where, OrderBy
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    public class HomeController : Controller
    {
        // 1. Khai báo biến để gọi Cơ sở dữ liệu
        private readonly DataMTTQContext _context; // Lưu ý: Nếu tên context của bạn khác, hãy sửa lại cho đúng nhé

        // 2. Tạo Constructor để tiêm DataMTTQContext vào Controller
        public HomeController(DataMTTQContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 3. Lấy danh sách Banner từ SQL: Chỉ lấy những Banner đang bật (TrangThai == true) và sắp xếp theo thứ tự
            var danhSachBanner = _context.Banners
                                         .Where(b => b.TrangThai == true)
                                         .OrderBy(b => b.ThuTu)
                                         .ToList();

            // 4. Gắn dữ liệu vào ViewModel
            var model = new HomePageViewModel
            {
                Banners = danhSachBanner
            };

            // 5. Trả Model chứa danh sách Banner về cho View hiển thị
            return View(model);
        }

        public IActionResult UnderConstruction()
        {
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}