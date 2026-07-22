using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("DiaDiemBanDo")]
[Index("DaXoa", Name = "IDX_DiaDiemBanDo_DaXoa")]
public partial class DiaDiemBanDo
{
    [Key]
    [Column("IDDiaDiem")]
    public int IddiaDiem { get; set; }

    [StringLength(500)]
    public string TenDiaDiem { get; set; } = null!;

    [StringLength(50)]
    public string PhanLoaiBanDo { get; set; } = null!;

    [Column(TypeName = "decimal(10, 8)")]
    public decimal ViDo { get; set; }

    [Column(TypeName = "decimal(10, 8)")]
    public decimal KinhDo { get; set; }

    [StringLength(500)]
    public string? DiaChi { get; set; }

    [StringLength(255)]
    public string? ThongTinLienHe { get; set; }

    [Column("IDDonVi")]
    public int? IddonVi { get; set; }

    public byte[]? HinhAnhThucTe { get; set; }

    public string? MoTaChiTiet { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    public bool? DaXoa { get; set; }

    [ForeignKey("IddonVi")]
    [InverseProperty("DiaDiemBanDos")]
    public virtual DoanTheToChuc? IddonViNavigation { get; set; }
}
