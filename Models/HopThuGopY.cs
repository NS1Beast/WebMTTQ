using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("HopThuGopY")]
public partial class HopThuGopY
{
    [Key]
    [Column("IDGopY")]
    public int IdgopY { get; set; }

    // === 2 TRƯỜNG THÊM MỚI ĐỂ KHỚP VỚI UI ===
    [Required(ErrorMessage = "Vui lòng chọn lĩnh vực góp ý")]
    [StringLength(100)]
    [Display(Name = "Lĩnh vực góp ý")]
    public string? LinhVuc { get; set; } // Lĩnh vực góp ý

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 250 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; } // Địa chỉ người gửi
    // =======================================

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề góp ý")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Tiêu đề phải từ 10 đến 500 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string TieuDe { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
    [Display(Name = "Họ tên")]
    public string? HoTen { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [Unicode(false)]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    [Unicode(false)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung góp ý")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "Nội dung phải từ 10 đến 10.000 ký tự")]
    [Display(Name = "Nội dung")]
    public string NoiDung { get; set; } = null!;

    public byte[]? TepMinhChung { get; set; }

    [StringLength(50)]
    [Display(Name = "Trạng thái")]
    public string? TrangThai { get; set; }

    [StringLength(10000)]
    [Display(Name = "Nội dung phản hồi")]
    public string? NoiDungPhanHoi { get; set; }

    [Column("IDNguoiXuLy")]
    [Display(Name = "Người xử lý")]
    public int? IdnguoiXuLy { get; set; }

    [Column(TypeName = "datetime")]
    [Display(Name = "Ngày gửi")]
    public DateTime? NgayGui { get; set; }

    public bool? DaXoa { get; set; }

    [ForeignKey("IdnguoiXuLy")]
    [InverseProperty("HopThuGopies")]
    public virtual NguoiDung? IdnguoiXuLyNavigation { get; set; }
}