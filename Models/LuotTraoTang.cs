using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("LuotTraoTang")]
public partial class LuotTraoTang
{
    [Key]
    [Column("IDTraoTang")]
    public int IdtraoTang { get; set; }

    [Column("IDChuongTrinh")]
    public int IdchuongTrinh { get; set; }

    [Column("IDNguoiCanGiup")]
    public int IdnguoiCanGiup { get; set; }

    [Column("IDQuy")]
    public int? Idquy { get; set; }

    [StringLength(50)]
    public string LoaiHoTro { get; set; } = null!;

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? SoTienHoTro { get; set; }

    [StringLength(255)]
    public string? TenHienVat { get; set; }

    [StringLength(255)]
    public string? QuyCach { get; set; }

    [Column("IDNguoiCap")]
    public int IdnguoiCap { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayPhanBo { get; set; }

    [ForeignKey("IdchuongTrinh")]
    [InverseProperty("LuotTraoTangs")]
    public virtual ChuongTrinhHoTro IdchuongTrinhNavigation { get; set; } = null!;

    [ForeignKey("IdnguoiCanGiup")]
    [InverseProperty("LuotTraoTangs")]
    public virtual NguoiCanGiupDo IdnguoiCanGiupNavigation { get; set; } = null!;

    [ForeignKey("IdnguoiCap")]
    [InverseProperty("LuotTraoTangs")]
    public virtual NguoiDung IdnguoiCapNavigation { get; set; } = null!;

    [ForeignKey("Idquy")]
    [InverseProperty("LuotTraoTangs")]
    public virtual DanhMucQuy? IdquyNavigation { get; set; }
}
