using System.ComponentModel.DataAnnotations;

namespace WebMTTQ.Models
{
    // ================================================
    // DANH SÁCH MODULE QUYỀN TRUY CẬP
    // ================================================
    public static class ModuleQuyen
    {
        public const string Dashboard = "dashboard";
        public const string TrangChu = "trangchu";
        public const string ThongTinUngHo = "thongtinungho";
        public const string DanhSachUngHo = "danhsachungho";
        public const string SoDuQuy = "soduquy";
        public const string KetQuaChamLo = "ketquachamlo";
        public const string DiaDiemBanDo = "diadiembando";
        public const string NguoiDanCanTroGiup = "nguoidancantrogium";
        public const string GopY = "gopy";
        public const string Banner = "banner";
        public const string CauHinh = "cauhinh";
        public const string QuanLyNguoiDung = "quanlynguoidung";

        public static List<ModuleInfo> GetAllModules()
        {
            return new List<ModuleInfo>
            {
                new ModuleInfo { MaModule = Dashboard, TenModule = "Dashboard", Icon = "fa-chart-pie", MoTa = "Bảng điều khiển tổng quan" },
                new ModuleInfo { MaModule = TrangChu, TenModule = "Cài đặt Trang chủ", Icon = "fa-home", MoTa = "Quản lý nội dung trang chủ" },
                new ModuleInfo { MaModule = ThongTinUngHo, TenModule = "Thông tin ủng hộ", Icon = "fa-university", MoTa = "Quản lý thông tin ủng hộ" },
                new ModuleInfo { MaModule = DanhSachUngHo, TenModule = "Danh sách ủng hộ", Icon = "fa-hand-holding-usd", MoTa = "Quản lý danh sách ủng hộ" },
                new ModuleInfo { MaModule = SoDuQuy, TenModule = "Số dư Quỹ", Icon = "fa-wallet", MoTa = "Quản lý số dư quỹ" },
                new ModuleInfo { MaModule = KetQuaChamLo, TenModule = "Kết quả chăm lo", Icon = "fa-hand-holding-heart", MoTa = "Quản lý kết quả chăm lo" },
                new ModuleInfo { MaModule = DiaDiemBanDo, TenModule = "Bản đồ an sinh", Icon = "fa-map-marked-alt", MoTa = "Quản lý bản đồ an sinh" },
                new ModuleInfo { MaModule = NguoiDanCanTroGiup, TenModule = "Yêu cầu trợ giúp", Icon = "fa-life-ring", MoTa = "Quản lý yêu cầu trợ giúp" },
                new ModuleInfo { MaModule = GopY, TenModule = "Hộp thư góp ý", Icon = "fa-envelope-open-text", MoTa = "Quản lý hộp thư góp ý" },
                new ModuleInfo { MaModule = Banner, TenModule = "Quản lý Banner", Icon = "fa-images", MoTa = "Quản lý banner" },
                new ModuleInfo { MaModule = CauHinh, TenModule = "Cài đặt hệ thống", Icon = "fa-cog", MoTa = "Cấu hình hệ thống" },
                new ModuleInfo { MaModule = QuanLyNguoiDung, TenModule = "Quản lý người dùng", Icon = "fa-users", MoTa = "Quản lý tài khoản và phân quyền" }
            };
        }
    }

    public class ModuleInfo
    {
        public string MaModule { get; set; } = string.Empty;
        public string TenModule { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }

    // ================================================
    // VIEW MODEL: DANH SÁCH NGƯỜI DÙNG
    // ================================================
    public class QuanLyNguoiDungIndexViewModel
    {
        public List<NguoiDungItem> NguoiDungs { get; set; } = new();
        public List<VaiTro> VaiTros { get; set; } = new();
        public string? TuKhoa { get; set; }
        public int? IdVaiTroFilter { get; set; }
    }

    public class NguoiDungItem
    {
        public int IdnguoiDung { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? TenVaiTro { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public int SoModuleDuocQuyen { get; set; }
        public bool LaAdmin { get; set; }
    }

    // ================================================
    // VIEW MODEL: TẠO NGƯỜI DÙNG
    // ================================================
    public class TaoNguoiDungViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có từ 3 đến 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string XacNhanMatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int IdVaiTro { get; set; }

        public List<VaiTro> VaiTros { get; set; } = new();
        public List<ModuleQuyenCheckbox> Modules { get; set; } = new();
    }

    // ================================================
    // VIEW MODEL: CHỈNH SỬA NGƯỜI DÙNG
    // ================================================
    public class SuaNguoiDungViewModel
    {
        public int IdnguoiDung { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới (để trống nếu không đổi)")]
        public string? MatKhauMoi { get; set; }

        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string? XacNhanMatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int IdVaiTro { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "HoatDong";

        public List<VaiTro> VaiTros { get; set; } = new();
        public List<ModuleQuyenCheckbox> Modules { get; set; } = new();
    }

    // ================================================
    // VIEW MODEL: QUẢN LÝ VAI TRÒ
    // ================================================
    public class VaiTroViewModel
    {
        public int IdvaiTro { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên vai trò")]
        public string TenVaiTro { get; set; } = string.Empty;

        public List<ModuleQuyenCheckbox> Modules { get; set; } = new();
    }

    // ================================================
    // VIEW MODEL: CHECKBOX QUYỀN TRUY CẬP MODULE
    // ================================================
    public class ModuleQuyenCheckbox
    {
        public string MaModule { get; set; } = string.Empty;
        public string TenModule { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public bool DuocChon { get; set; }
        public bool CoQuyenXem { get; set; }
        public bool CoQuyenThem { get; set; }
        public bool CoQuyenSua { get; set; }
        public bool CoQuyenXoa { get; set; }
    }

    // ================================================
    // VIEW MODEL: TRANG CÁ NHÂN
    // ================================================
    public class ThongTinCaNhanViewModel
    {
        public int IdnguoiDung { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        public string? TenVaiTro { get; set; }
        public DateTime? NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public byte[]? AnhDaiDien { get; set; }

        public List<string> ModulesDuocQuyen { get; set; } = new();
    }

    // ================================================
    // VIEW MODEL: ĐỔI MẬT KHẨU (TRANG CÁ NHÂN)
    // ================================================
    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }

    // ================================================
    // VIEW MODEL: QUÊN MẬT KHẨU - BƯỚC 1 (NHẬP EMAIL)
    // ================================================
    public class QuenMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    // ================================================
    // VIEW MODEL: XÁC NHẬN OTP
    // ================================================
    public class XacNhanOtpViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 số")]
        [Display(Name = "Mã OTP")]
        public string MaOtp { get; set; } = string.Empty;
    }

    // ================================================
    // VIEW MODEL: ĐẶT LẠI MẬT KHẨU (SAU KHI XÁC NHẬN OTP)
    // ================================================
    public class DatLaiMatKhauViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string MaOtp { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; } = string.Empty;
    }

    // ================================================
    // VIEW MODEL: CẤU HÌNH SMTP EMAIL
    // ================================================
    public class CauHinhEmailViewModel
    {
        [Required(ErrorMessage = "SMTP Host không được để trống")]
        [Display(Name = "SMTP Host")]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "Port không hợp lệ")]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; } = 587;

        [Display(Name = "Bật SSL/TLS")]
        public bool SmtpUseSsl { get; set; } = true;

        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "SMTP Password")]
        public string? SmtpPassword { get; set; }

        [Display(Name = "SMTP Password (đã cấu hình)")]
        public string? SmtpPassword_Display { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email gửi (From)")]
        public string SmtpFromEmail { get; set; } = string.Empty;

        [Display(Name = "Tên hiển thị (From)")]
        public string SmtpFromName { get; set; } = string.Empty;
    }
}