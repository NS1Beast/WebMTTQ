using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("NguoiDung")]
[Index("TenDangNhap", Name = "UQ__NguoiDun__55F68FC0CA9219EA", IsUnique = true)]
[Index("Email", Name = "UQ__NguoiDun__A9D105348E5266DA", IsUnique = true)]
public partial class NguoiDung
{
    [Key]
    [Column("IDNguoiDung")]
    public int IdnguoiDung { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string MatKhau { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    public byte[]? AnhDaiDien { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [Column("IDVaiTro")]
    public int? IdvaiTro { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    public bool? DaXoa { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("IdnguoiDungNavigation")]
    public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();

    [InverseProperty("IdnguoiXuLyNavigation")]
    public virtual ICollection<DonXinHoTro> DonXinHoTros { get; set; } = new List<DonXinHoTro>();

    [InverseProperty("IdnguoiXuLyNavigation")]
    public virtual ICollection<HopThuGopY> HopThuGopies { get; set; } = new List<HopThuGopY>();

    [ForeignKey("IdvaiTro")]
    [InverseProperty("NguoiDungs")]
    public virtual VaiTro? IdvaiTroNavigation { get; set; }

    [InverseProperty("IdnguoiTiepNhanNavigation")]
    public virtual ICollection<KhoanDongGop> KhoanDongGops { get; set; } = new List<KhoanDongGop>();

    [InverseProperty("IdnguoiCapNavigation")]
    public virtual ICollection<LuotTraoTang> LuotTraoTangs { get; set; } = new List<LuotTraoTang>();

    [InverseProperty("IdnguoiDungNavigation")]
    public virtual ICollection<NhatKyHeThong> NhatKyHeThongs { get; set; } = new List<NhatKyHeThong>();
}
