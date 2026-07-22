using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("ChuongTrinhHoTro")]
public partial class ChuongTrinhHoTro
{
    [Key]
    [Column("IDChuongTrinh")]
    public int IdchuongTrinh { get; set; }

    [StringLength(500)]
    public string TenChuongTrinh { get; set; } = null!;

    [StringLength(50)]
    public string? LoaiHinh { get; set; }

    public string? MoTa { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    public bool? DaXoa { get; set; }

    [InverseProperty("IdchuongTrinhNavigation")]
    public virtual ICollection<LuotTraoTang> LuotTraoTangs { get; set; } = new List<LuotTraoTang>();
}
