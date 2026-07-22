using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("NhatKyHeThong")]
public partial class NhatKyHeThong
{
    [Key]
    [Column("IDNhatKy")]
    public int IdnhatKy { get; set; }

    [Column("IDNguoiDung")]
    public int? IdnguoiDung { get; set; }

    [StringLength(50)]
    public string HanhDong { get; set; } = null!;

    [StringLength(50)]
    public string TenBang { get; set; } = null!;

    [Column("IDBanGhi")]
    public int? IdbanGhi { get; set; }

    public string? DuLieuCu { get; set; }

    public string? DuLieuMoi { get; set; }

    [Column("DiaChiIP")]
    [StringLength(50)]
    [Unicode(false)]
    public string? DiaChiIp { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ThoiGianTao { get; set; }

    [ForeignKey("IdnguoiDung")]
    [InverseProperty("NhatKyHeThongs")]
    public virtual NguoiDung? IdnguoiDungNavigation { get; set; }
}
