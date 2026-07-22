using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        return View(BuildAboutPage());
    }

    private static AboutPageViewModel BuildAboutPage() => new()
    {
        FoundContent = @"Ngày <strong>01/7/2025</strong>, tại cuộc họp Hội đồng nhân dân PHƯỜNG TÂN ĐỊNH, đồng chí <strong>Nguyễn Văn Nghĩa</strong> — Bí thư Đảng ủy xã đã công bố <strong>Quyết định số 291/QĐ-MTTQ-BTT</strong> ngày 27/6/2025 của Ban Thường trực Ủy ban MTTQ Việt Nam Thành phố Hồ Chí Minh về việc thành lập Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH, công nhận Ủy viên Ủy ban và Ban Thường trực.

Xin trân trọng giới thiệu đến toàn thể nhân dân, đoàn viên, hội viên và các bạn đoàn viên, thanh thiếu nhi trên địa bàn xã các đồng chí trong Ban Thường trực Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH.",

        Stats =
        [
            new() { Count = 5, Suffix = "", Label = "Đồng chí Ban Thường trực" },
            new() { Count = 5, Suffix = "", Label = "Tổ chức thành viên hợp thành" },
            new() { Count = 100, Suffix = "%", Label = "Gần dân, sát dân, vì nhân dân" }
        ],

        Chairman = new TeamMember
        {
            Name = "Nguyễn Thị Thuận",
            Role = "Ủy viên Ban Thường vụ Đảng ủy, Chủ tịch Ủy ban MTTQ Việt Nam xã",
            Pill = "Chủ tịch",
            PhotoUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/nguyen-thi-thuan-768x1024.jpg",
            PhotoFullUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/nguyen-thi-thuan.jpg",
            PhotoPosition = "center 22%",
            IsChairman = true
        },

        Members =
        [
            new TeamMember
            {
                Name = "Trần Thị Thanh Huyền",
                Role = "Đảng ủy viên, Phó Chủ tịch Thường trực Ủy ban MTTQ Việt Nam, Chủ tịch Hội Phụ nữ xã",
                PhotoUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/tran-thi-thanh-huyen-768x1152.jpg",
                PhotoFullUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/tran-thi-thanh-huyen.jpg",
                PhotoPosition = "center 42%"
            },
            new TeamMember
            {
                Name = "Nguyễn Huỳnh Quang",
                Role = "Đảng ủy viên, Phó Chủ tịch Ủy ban MTTQ Việt Nam, Chủ tịch Công đoàn xã",
                PhotoUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/nguyen-huynh-quang-768x1024.jpg",
                PhotoFullUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/nguyen-huynh-quang.jpg",
                PhotoPosition = "center 22%"
            },
            new TeamMember
            {
                Name = "Lương Đức Toản",
                Role = "Đảng ủy viên, Phó Chủ tịch Ủy ban MTTQ Việt Nam, Chủ tịch Hội Cựu chiến binh xã",
                PhotoUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/luong-duc-toan.jpg",
                PhotoFullUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/luong-duc-toan.jpg",
                PhotoPosition = "center 22%"
            },
            new TeamMember
            {
                Name = "Trần Đỗ Xuân Thương",
                Role = "Đảng ủy viên, Phó Chủ tịch Ủy ban MTTQ Việt Nam, Bí thư Đoàn Thanh niên xã",
                PhotoUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/tran-do-xuan-thuong-768x1152.jpg",
                PhotoFullUrl = "https://mttq.phuhoadong.vn/apps/uploads/2026/07/tran-do-xuan-thuong-scaled.jpg",
                PhotoPosition = "center 22%"
            }
        ],

        GalleryImageUrl = "https://mttq.phuhoadong.vn/apps/uploads/2025/10/z7148534126458_f9964a73f6fb1ceac44c47b2c52629b8-1024x683.jpg",
        GalleryImageAlt = "Ban Thường trực Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH ra mắt",

        ShareUrl = "/gioi-thieu/",
        ShareTitle = "Giới thiệu - Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH",

        CtaDocsUrl = "#",
        CtaFeedbackUrl = "#"
    };
}