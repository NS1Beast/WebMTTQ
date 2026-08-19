using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("KhoanDongGop")]
public partial class KhoanDongGop
{
    [Key]
    [Column("IDGiaoDich")]
    public int IdgiaoDich { get; set; }

    [Column("IDNhaHaoTam")]
    public int IdnhaHaoTam { get; set; }

    [Column("IDQuy")]
    public int? Idquy { get; set; }

    [Column("IDNguoiTiepNhan")]
    public int IdnguoiTiepNhan { get; set; }

    [StringLength(50)]
    public string LoaiUngHo { get; set; } = null!;

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? SoTien { get; set; }

    [StringLength(255)]
    public string? TenHienVat { get; set; }

    [StringLength(255)]
    public string? QuyCach { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayUngHo { get; set; }

    [ForeignKey("IdnguoiTiepNhan")]
    [InverseProperty("KhoanDongGops")]
    public virtual NguoiDung IdnguoiTiepNhanNavigation { get; set; } = null!;

    [ForeignKey("IdnhaHaoTam")]
    [InverseProperty("KhoanDongGops")]
    public virtual NhaHaoTam IdnhaHaoTamNavigation { get; set; } = null!;

    [ForeignKey("Idquy")]
    [InverseProperty("KhoanDongGops")]
    public virtual DanhMucQuy? IdquyNavigation { get; set; }
}
