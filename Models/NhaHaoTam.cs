using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("NhaHaoTam")]
public partial class NhaHaoTam
{
    [Key]
    [Column("IDNhaHaoTam")]
    public int IdnhaHaoTam { get; set; }

    [StringLength(255)]
    public string TenNguoiUngHo { get; set; } = null!;

    [StringLength(50)]
    public string? LoaiHinh { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? DiaChi { get; set; }

    [InverseProperty("IdnhaHaoTamNavigation")]
    public virtual ICollection<KhoanDongGop> KhoanDongGops { get; set; } = new List<KhoanDongGop>();
}
