using Microsoft.AspNetCore.Mvc;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        var model = new AboutPageViewModel
        {
            PageTitle = "Giới thiệu",
            HeroTitle = "ỦY BAN MẶT TRẬN TỔ QUỐC VIỆT NAM PHƯỜNG<br>và các <span style='color: #c4283f;'>TỔ CHỨC CHÍNH TRỊ - XÃ HỘI</span>",
            HeroSubtitle = "Ủy ban Mặt trận Tổ quốc Việt Nam phường và các tổ chức chính trị - xã hội là lực lượng nòng cốt trong việc xây dựng khối đại đoàn kết toàn dân tộc, phát huy quyền làm chủ của Nhân dân, đại diện, bảo vệ quyền và lợi ích hợp pháp, chính đáng của đoàn viên, hội viên và Nhân dân. Với mô hình hoạt động thống nhất, gồm Ủy ban MTTQ Việt Nam, Đoàn Thanh niên Cộng sản Hồ Chí Minh, Hội Liên hiệp Phụ nữ, Hội Nông dân và Hội Cựu chiến binh, các tổ chức luôn phối hợp chặt chẽ trong công tác tuyên truyền, vận động Nhân dân, thực hiện các phong trào thi đua yêu nước, các cuộc vận động, giám sát, phản biện xã hội và tham gia xây dựng Đảng, chính quyền trong sạch, vững mạnh.",
            
            // Chairman
            Chairman = new TeamMember
            {
                Name = "Bà NGUYỄN ĐINH MINH PHƯƠNG",
                Role = "Ủy viên Ban Thường vụ Đảng ủy<br><strong>CHỦ TỊCH</strong><br>Ủy ban Mặt trận Tổ quốc Việt Nam phường",
                Pill = "CHỦ TỊCH",
                PhotoUrl = "",
                PhotoFullUrl = "",
                IsChairman = true
            },
            
            // Organization members
            Members = new List<TeamMember>
            {
                new TeamMember
                {
                    Name = "Bà PHAN THỊ NHƯ LINH",
                    Role = "Đảng ủy viên<br>Phó Chủ tịch Thường trực<br>Ủy ban MTTQ Việt Nam phường<br><strong>CHỦ TỊCH</strong><br>Hội Liên hiệp Phụ nữ phường",
                    Pill = "CHỦ TỊCH",
                    PhotoUrl = "",
                    PhotoFullUrl = "",
                    IsChairman = false
                },
                new TeamMember
                {
                    Name = "Bà HÀ THỊ THU ĐỊNH",
                    Role = "Đảng ủy viên<br>Phó Chủ tịch<br>Ủy ban MTTQ Việt Nam phường<br><strong>CHỦ TỊCH</strong><br>Công đoàn phường",
                    Pill = "CHỦ TỊCH",
                    PhotoUrl = "",
                    PhotoFullUrl = "",
                    IsChairman = false
                },
                new TeamMember
                {
                    Name = "Ông TRẦN VĂN HÙNG",
                    Role = "Phó Chủ tịch<br>Ủy ban MTTQ Việt Nam phường<br><strong>CHỦ TỊCH</strong><br>Hội Cựu chiến binh phường",
                    Pill = "CHỦ TỊCH",
                    PhotoUrl = "",
                    PhotoFullUrl = "",
                    IsChairman = false
                },
                new TeamMember
                {
                    Name = "Ông BÙI VIỆT HẢI",
                    Role = "Phó Chủ tịch<br>Ủy ban MTTQ Việt Nam phường<br><strong>BÍ THƯ</strong><br>Đoàn Thanh niên Cộng sản Hồ Chí Minh phường",
                    Pill = "BÍ THƯ",
                    PhotoUrl = "",
                    PhotoFullUrl = "",
                    IsChairman = false
                }
            }
        };

        return View(model);
    }
}
