using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("DanhMucQuy")]
public partial class DanhMucQuy
{
    [Key]
    [Column("IDQuy")]
    public int Idquy { get; set; }

    [StringLength(255)]
    public string TenQuy { get; set; } = null!;

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? TongThu { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? TongChi { get; set; }

    [Column(TypeName = "decimal(18, 0)")]
    public decimal? TonQuy { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [InverseProperty("IdquyNavigation")]
    public virtual ICollection<KhoanDongGop> KhoanDongGops { get; set; } = new List<KhoanDongGop>();

    [InverseProperty("IdquyNavigation")]
    public virtual ICollection<LuotTraoTang> LuotTraoTangs { get; set; } = new List<LuotTraoTang>();
}
