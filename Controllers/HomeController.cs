using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View(BuildHomePage());
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static HomePageViewModel BuildHomePage() => new()
    {
        LeadNews = new FeaturedNewsItem
        {
            Title = "MTTQ Việt Nam và Nhân dân tham gia giám sát việc thực hiện chức trách, nhiệm vụ của Công an cấp xã",
            Url = "#",
            ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/747977948_1360530989349470_8826317708722677423_n.jpg",
            Category = "Dân chủ - pháp luật",
            Date = "20/07/2026"
        },
        SideNews =
        [
            new NewsItem
            {
                Title = "Infographic những điểm chính về tổ chức và hoạt động của khu phố, ấp, thôn, khu dân cư trên địa bàn TP.HCM",
                Url = "#",
                ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/749609118_1749991869487246_5439748495454013973_n-273x410.jpg",
                Category = "Dân chủ - pháp luật",
                Date = "20/07/2026"
            },
            new NewsItem
            {
                Title = "TRỢ LÝ AI XÃ PHÚ HÒA ĐÔNG – TRA CỨU THÔNG TIN NHANH CHÓNG, CHÍNH XÁC",
                Url = "#",
                ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/752524597_1351461883758021_5443031646711912290_n-410x410.jpg",
                Category = "Tin tức",
                Date = "20/07/2026"
            },
            new NewsItem
            {
                Title = "QUY ĐỊNH TỔ CHỨC VÀ HOẠT ĐỘNG CỦA BAN CÔNG TÁC MẶT TRẬN KHU DÂN CƯ",
                Url = "#",
                ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/image-1-274x410.png",
                Category = "Tin tức",
                Date = "10/07/2026"
            },
            new NewsItem
            {
                Title = "Tiêu chuẩn Trưởng Ban Công tác Mặt trận và quy định hoạt động của Ban Công tác Mặt trận tại khu dân cư",
                Url = "#",
                ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/4_42.webp",
                Category = "Tin tức",
                Date = "09/07/2026"
            }
        ],
        Categories =
        [
            new NewsCategoryBlock
            {
                Title = "Vận động và phong trào",
                MoreUrl = "#",
                Featured = new FeaturedNewsItem
                {
                    Title = "Nhiều hoạt động ý nghĩa chào mừng kỷ niệm 50 năm Ngày TP Sài Gòn – Gia Định mang tên Chủ tịch Hồ Chí Minh",
                    Url = "#",
                    ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/06/image-13-685x1024.png",
                    Date = "26/06/2026"
                },
                ListItems =
                [
                    new() { Title = "Công tác hỗ trợ đột xuất từ Quỹ \"Vì người nghèo\" xã Phú Hòa Đông", Url = "#", Date = "14/06/2026" },
                    new() { Title = "RA QUÂN CHỦ NHẬT XANH LẦN THỨ 166", Url = "#", Date = "24/05/2026" },
                    new() { Title = "HỘI THI TÌM HIỂU KHÔNG GIAN VĂN HÓA HỒ CHÍ MINH NĂM 2026", Url = "#", Date = "13/05/2026" },
                    new() { Title = "CHÀO MỪNG KỶ NIỆM 51 NĂM NGÀY GIẢI PHÓNG MIỀN NAM, THỐNG NHẤT ĐẤT NƯỚC", Url = "#", Date = "14/04/2026" },
                    new() { Title = "CHƯƠNG TRÌNH TRAO TẶNG QUÀ, HỌC BỔNG CHO HỌC SINH ĐỒNG BÀO DÂN TỘC KHMER", Url = "#", Date = "12/04/2026" }
                ]
            },
            new NewsCategoryBlock
            {
                Title = "Tin tức",
                MoreUrl = "#",
                Featured = new FeaturedNewsItem
                {
                    Title = "THÔNG BÁO NHÂN SỰ BÍ THƯ CHI BỘ, PHÓ BÍ THƯ/TRƯỞNG ẤP TRÊN ĐỊA BÀN XÃ PHÚ HÒA ĐÔNG (SAU SÁP NHẬP)",
                    Url = "#",
                    ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/739391062_4532336506986380_4725244824577672906_n-1-1024x576.jpg",
                    Date = "07/07/2026"
                },
                ListItems =
                [
                    new() { Title = "LỄ CHÀO CỜ ĐẦU TUẦN – KỂ CHUYỆN HỌC TẬP VÀ LÀM THEO BÁC", Url = "#", Date = "07/07/2026" },
                    new() { Title = "ĐẢNG ỦY CÁC CƠ QUAN ĐẢNG XÃ PHÚ HÒA ĐÔNG SƠ KẾT CÔNG TÁC XÂY DỰNG ĐẢNG", Url = "#", Date = "07/07/2026" },
                    new() { Title = "BÌNH DÂN HỌC VỤ SỐ – KỲ 04: ỨNG DỤNG CÔNG NGHỆ VÀO ĐỜI SỐNG", Url = "#", Date = "06/07/2026" },
                    new() { Title = "HỘI NGHỊ ỦY VIÊN ỦY BAN MTTQ VIỆT NAM XÃ LẦN THỨ IV (MỞ RỘNG)", Url = "#", Date = "03/07/2026" },
                    new() { Title = "HỘI NGHỊ QUÁN TRIỆT NGHỊ QUYẾT SỐ 09-NQ/TW CỦA BỘ CHÍNH TRỊ", Url = "#", Date = "03/07/2026" }
                ]
            },
            new NewsCategoryBlock
            {
                Title = "Lời Bác dạy",
                MoreUrl = "#",
                StyleClass = "style2",
                GridItems =
                [
                    new() { Title = "LỜI BÁC DẠY NGÀY NÀY NĂM XƯA, NGÀY 08 THÁNG 7", Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/BAC-HO-1024x769.jpg", Date = "08/07/2026" },
                    new() { Title = "LỜI BÁC DẠY NGÀY NÀY NĂM XƯA, NGÀY 07 THÁNG 7", Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/BAC-HO-1024x769.jpg", Date = "07/07/2026" },
                    new() { Title = "LỜI BÁC DẠY NGÀY NÀY NĂM XƯA, NGÀY 06 THÁNG 7", Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/BAC-HO-1024x769.jpg", Date = "06/07/2026" },
                    new() { Title = "LỜI BÁC DẠY NGÀY NÀY NĂM XƯA, NGÀY 05 THÁNG 7", Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/BAC-HO-1024x769.jpg", Date = "05/07/2026" },
                    new() { Title = "LỜI BÁC DẠY NGÀY NÀY NĂM XƯA, NGÀY 03 THÁNG 7", Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/06/image-8-1024x769.png", Date = "03/07/2026" },
                    new() { Title = "LỜI BÁC DẠY NGÀY NÀY NĂM XƯA, NGÀY 02 THÁNG 7", Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/06/image-7-1024x769.png", Date = "02/07/2026" }
                ]
            }
        ],
        Timeline =
        [
            new() { Date = "12/8/2025", Title = "Bản đồ số xã Phú Hòa Đông", Description = "Số hóa địa giới, trụ sở và các điểm quan trọng trên nền bản đồ tương tác." },
            new() { Date = "11/9/2025", Title = "Zalo OA Mặt trận xã Phú Hòa Đông", Description = "Kênh Zalo chính thức kết nối, đưa thông tin nhanh đến người dân." },
            new() { Date = "11/10/2025", Title = "Hộp thư góp ý xây dựng Đảng, xây dựng chính quyền", Description = "Tiếp nhận phản ánh, kiến nghị trực tuyến và công khai tiến độ xử lý." },
            new() { Date = "26/10/2025", Title = "Trang thông tin Mặt trận xã Phú Hòa Đông", Description = "Cổng thông tin điện tử công khai văn bản và hoạt động của Mặt trận." },
            new() { Date = "2/2/2026", Title = "Không gian văn hóa Hồ Chí Minh", Description = "Thư viện, tư liệu, âm nhạc học tập và làm theo tư tưởng Bác Hồ." },
            new() { Date = "26/2/2026", Title = "Bản đồ tra cứu khu vực bầu cử", Description = "Giúp cử tri tra cứu nhanh khu vực bỏ phiếu theo từng ấp, khu dân cư." },
            new() { Date = "26/3/2026", Title = "Thư viện số xã Phú Hòa Đông", Description = "Kho sách, tài liệu số phục vụ học tập và tra cứu của người dân." },
            new() { Date = "11/4/2026", Title = "Cổng thông tin an sinh xã hội", Description = "Số hóa dữ liệu hộ chính sách, hỗ trợ theo dõi và chăm lo kịp thời." },
            new() { Date = "21/4/2026", Title = "Trợ lý AI và Cổng dữ liệu số", Description = "Hỏi – đáp tự động 24/7 và tập trung toàn bộ nền tảng số của xã." },
            new() { Date = "26/6/2026", Title = "Bản đồ số các điểm sinh hoạt Hè", Description = "Định vị các điểm sinh hoạt, vui chơi Hè an toàn cho thiếu nhi." },
            new() { Date = "Dự kiến · 7/2026", Title = "Mini app Mặt trận xã Phú Hòa Đông", Description = "Ứng dụng trên Zalo tích hợp toàn bộ tiện ích số của xã trong một chạm.", IsNext = true }
        ],
        Stats =
        [
            new() { Count = 4820, Suffix = "+", Label = "Hộ dân được kết nối an sinh" },
            new() { Count = 98, Suffix = "%", Label = "Phản ánh xử lý đúng hạn" },
            new() { Count = 356, Label = "Văn bản công khai" },
            new() { Count = 127, Suffix = "K", Label = "Lượt truy cập cổng" }
        ]
    };
}
