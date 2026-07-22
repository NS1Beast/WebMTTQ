using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers;

public class NewsController : Controller
{
    public IActionResult Index(string category = "tin-tuc", int page = 1)
    {
        return View(BuildNewsPage(category, page));
    }

    private static NewsIndexViewModel BuildNewsPage(string currentCategory, int page)
    {
        var categories = new List<NewsCategoryInfo>
        {
            new() { Slug = "tin-tuc", Title = "Tin tức", Url = "/News?category=tin-tuc" },
            new() { Slug = "dan-chu-phap-luat", Title = "Dân chủ – pháp luật", Url = "/News?category=dan-chu-phap-luat" },
            new() { Slug = "giam-sat-va-phan-bien", Title = "Giám sát và phản biện", Url = "/News?category=giam-sat-va-phan-bien" },
            new() { Slug = "bao-ve-nen-tang-tu-tuong-dang", Title = "Bảo vệ nền tảng tư tưởng của Đảng (Cộng Sản Việt Nam)", Url = "/News?category=bao-ve-nen-tang-tu-tuong-dang" },
            new() { Slug = "hoat-dong-to-chuc-thanh-vien", Title = "Hoạt động của các tổ chức thành viên", Url = "/News?category=hoat-dong-to-chuc-thanh-vien" },
            new() { Slug = "tuyen-truyen-van-dong-nhan-dan", Title = "Tuyên truyền vận động nhân dân", Url = "/News?category=tuyen-truyen-van-dong-nhan-dan" },
            new() { Slug = "van-dong-va-phong-trao", Title = "Vận động và phong trào", Url = "/News?category=van-dong-va-phong-trao" }
        };

        foreach (var cat in categories)
            cat.IsActive = cat.Slug == currentCategory;

        var allArticles = new List<NewsArticleItem>
        {
            new() { Title = "TRỢ LÝ AI PHƯỜNG TÂN ĐỊNH – TRA CỨU THÔNG TIN NHANH CHÓNG, CHÍNH XÁC",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/752524597_1351461883758021_5443031646711912290_n-410x410.jpg",
                    Date = "20/07/2026",
                    Excerpt = "Nhằm đẩy mạnh chuyển đổi số và nâng cao chất lượng phục vụ Nhân dân, Trợ lý AI PHƯỜNG TÂN ĐỊNH được đưa vào hoạt động, hỗ trợ người…" },
            new() { Title = "QUY ĐỊNH TỔ CHỨC VÀ HOẠT ĐỘNG CỦA BAN CÔNG TÁC MẶT TRẬN KHU DÂN CƯ",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/image-1-274x410.png",
                    Date = "10/07/2026",
                    Excerpt = "Căn cứ theo Hướng dẫn số 11/HD-MTTW-BTT ngày 10/9/2025 của Ban Thường trực Ủy ban Trung ương MTTQ Việt Nam về một số nội dung về tổ chức và hoạt…" },
            new() { Title = "Tiêu chuẩn Trưởng Ban Công tác Mặt trận và quy định hoạt động của Ban Công tác Mặt trận tại khu dân cư",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/4_42.webp",
                    Date = "09/07/2026",
                    Excerpt = "Ban Công tác Mặt trận ở khu dân cư đóng vai trò hạt nhân trong việc củng cố khối đại đoàn kết toàn dân tộc, là cầu nối trực tiếp…" },
            new() { Title = "THÔNG BÁO NHÂN SỰ BÍ THƯ CHI BỘ, PHÓ BÍ THƯ/TRƯỞNG ẤP, CHI UỶ VIÊN/TRƯỞNG BAN CÔNG TÁC MẶT TRẬN ẤP VÀ VĂN PHÒNG ẤP TRÊN ĐỊA BÀN XÃ PHÚ HOÀ ĐÔNG (SAU SÁP NHẬP)",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/739391062_4532336506986380_4725244824577672906_n-1-729x410.jpg",
                    Date = "07/07/2026",
                    Excerpt = "Thông báo nhân sự bí thư chi bộ, phó bí thư/trưởng ấp, chi uỷ viên/trưởng ban công tác mặt trận ấp và văn phòng ấp trên địa bàn xã phú…" },
            new() { Title = "LỄ CHÀO CỜ ĐẦU TUẦN – KỂ CHUYỆN “HỌC TẬP VÀ LÀM THEO TƯ TƯỞNG, ĐẠO ĐỨC, PHONG CÁCH HỒ CHÍ MINH”",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/739470725_1341343411436535_4664380054055574834_n-308x410.jpg",
                    Date = "07/07/2026",
                    Excerpt = "Sáng ngày 06/7/2026, Đảng ủy PHƯỜNG TÂN ĐỊNH tổ chức Lễ chào cờ đầu tuần kết hợp kể chuyện gương sáng học tập và làm theo tư tưởng,…" },
            new() { Title = "ĐẢNG ỦY CÁC CƠ QUAN ĐẢNG PHƯỜNG TÂN ĐỊNH TỔ CHỨC HỘI NGHỊ SƠ KẾT CÔNG TÁC XÂY DỰNG ĐẢNG 6 THÁNG ĐẦU NĂM",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/740259719_1341351328102410_806371656063046808_n-707x410.jpg",
                    Date = "07/07/2026",
                    Excerpt = "Sáng ngày 06/7/2026, Đảng ủy Các cơ quan Đảng PHƯỜNG TÂN ĐỊNH đã tổ chức Hội nghị sơ kết công tác xây dựng Đảng 6 tháng đầu năm…" },
            new() { Title = "BÌNH DÂN HỌC VỤ SỐ – KỲ 04: ỨNG DỤNG CÔNG NGHỆ VÀO ĐỜI SỐNG",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/chuyen-doi-so-co-hoi-va-thach-thuc-1-643x410.jpg",
                    Date = "06/07/2026",
                    Excerpt = "Công nghệ số đang hiện diện trong hầu hết các hoạt động hằng ngày, từ học tập, làm việc, mua sắm, thanh toán, chăm sóc sức khỏe đến kết nối…" },
            new() { Title = "HỘI NGHỊ ỦY VIÊN ỦY BAN MTTQ VIỆT NAM XÃ LẦN THỨ IV (MỞ RỘNG)",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/1783044129483_1867755137472471998_2962436025635005513_6b906a8cca50fdeca248f8d8b9d0144a-709x410.jpg",
                    Date = "03/07/2026",
                    Excerpt = "Chiều ngày 02/7/2026, Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH tổ chức Hội nghị Ủy viên Ủy ban MTTQ Việt Nam xã lần thứ IV (mở rộng). Tại…" },
            new() { Title = "HỘI NGHỊ QUÁN TRIỆT NGHỊ QUYẾT SỐ 09-NQ/TW CỦA BỘ CHÍNH TRỊ VÀ NGHỊ QUYẾT ĐẠI HỘI ĐẠI BIỂU TOÀN QUỐC MTTQ VIỆT NAM LẦN THỨ XI",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/1783041820094_1867755137472471998_2962436025635005513_e82aa83954b325cee1cb5dccb7012b16-658x410.jpg",
                    Date = "03/07/2026",
                    Excerpt = "Chiều ngày 02/7/2026, Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH đã tổ chức Hội nghị quán triệt, triển khai thực hiện Nghị quyết số 09-NQ/TW của Bộ Chính…" },
            new() { Title = "Hội nghị trực tuyến toàn quốc sơ kết 01 năm 6 tháng triển khai thực hiện Nghị quyết số 57-NQ/TW của Bộ Chính trị",
                    Url = "#", ImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/734808039_1336929155211294_8297172197444596856_n-615x410.jpg",
                    Date = "02/07/2026",
                    Excerpt = "Chiều 01/7, PHƯỜNG TÂN ĐỊNH tham dự Hội nghị trực tuyến toàn quốc sơ kết 01 năm 6 tháng triển khai thực hiện Nghị quyết số 57-NQ/TW ngày 22…" }
        };

        return new NewsIndexViewModel
        {
            CurrentCategory = currentCategory,
            Categories = categories,
            Articles = allArticles,
            Pagination = new NewsPaginationInfo
            {
                CurrentPage = page,
                TotalPages = 26,
                BaseUrl = $"/News?category={currentCategory}"
            },
            RecentDocs =
            [
                new() { Title = "Kế hoạch \"Phối hợp, giám sát phản biện xã hội… giai đoạn 2026 – 2031\"", Url = "#" },
                new() { Title = "Hướng dẫn số 19/HD-MTTQ-BTT về Sắp xếp, kiện toàn Ban Công tác Mặt trận ở khu phố, ấp", Url = "#" },
                new() { Title = "Quy định số 02/QĐi-MTTQ-BTT về Tiêu chuẩn Trưởng Ban Công tác Mặt trận", Url = "#" },
                new() { Title = "Hướng dẫn Số 03-HD/BTCTW về sắp xếp, thành lập tổ chức đảng ở thôn, tổ dân phố", Url = "#" },
                new() { Title = "CHỈ THỊ SỐ 45/CT-UBND về triển khai Kết luận số 18-KL/TW", Url = "#" }
            ]
        };
    }
}