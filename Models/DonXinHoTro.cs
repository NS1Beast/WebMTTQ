using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("DonXinHoTro")]
public partial class DonXinHoTro
{
    [Key]
    [Column("IDDon")]
    public int Iddon { get; set; }

    [StringLength(255)]
    public string HoTenNguoiGui { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string SoDienThoai { get; set; } = null!;

    [StringLength(500)]
    public string DiaChi { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    [StringLength(50)]
    public string? MucDoUuTien { get; set; }

    [Column("IDNguoiCanGiup")]
    public int? IdnguoiCanGiup { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [Column("IDNguoiXuLy")]
    public int? IdnguoiXuLy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayGui { get; set; }

    public bool? DaXoa { get; set; }

    [ForeignKey("IdnguoiCanGiup")]
    [InverseProperty("DonXinHoTros")]
    public virtual NguoiCanGiupDo? IdnguoiCanGiupNavigation { get; set; }

    [ForeignKey("IdnguoiXuLy")]
    [InverseProperty("DonXinHoTros")]
    public virtual NguoiDung? IdnguoiXuLyNavigation { get; set; }
}
