using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("ThanhPhanGiaoDien")]
public partial class ThanhPhanGiaoDien
{
    [Key]
    [Column("IDThanhPhan")]
    public int IdthanhPhan { get; set; }

    [StringLength(255)]
    public string TenHienThi { get; set; } = null!;

    public byte[]? HinhAnh { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? DuongDanLienKet { get; set; }

    [StringLength(50)]
    public string? Loai { get; set; }

    [StringLength(50)]
    public string? ViTri { get; set; }

    public int? ThuTu { get; set; }

    public bool? DaXoa { get; set; }
}
