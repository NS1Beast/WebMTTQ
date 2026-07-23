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
    [StringLength(100)]
    public string? LinhVuc { get; set; } // Lĩnh vực góp ý

    [StringLength(250)]
    public string? DiaChi { get; set; } // Địa chỉ người gửi
    // =======================================

    [StringLength(500)]
    public string TieuDe { get; set; } = null!;

    [StringLength(100)]
    public string? HoTen { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    public string NoiDung { get; set; } = null!;

    public byte[]? TepMinhChung { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    public string? NoiDungPhanHoi { get; set; }

    [Column("IDNguoiXuLy")]
    public int? IdnguoiXuLy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayGui { get; set; }

    public bool? DaXoa { get; set; }

    [ForeignKey("IdnguoiXuLy")]
    [InverseProperty("HopThuGopies")]
    public virtual NguoiDung? IdnguoiXuLyNavigation { get; set; }
}