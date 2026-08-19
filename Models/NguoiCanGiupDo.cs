using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("NguoiCanGiupDo")]
public partial class NguoiCanGiupDo
{
    [Key]
    [Column("IDNguoiCanGiup")]
    public int IdnguoiCanGiup { get; set; }

    [StringLength(255)]
    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    [Column("CCCD")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Cccd { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? SoDienThoai { get; set; }

    [StringLength(500)]
    public string? DiaChi { get; set; }

    public string? HoanCanh { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [InverseProperty("IdnguoiCanGiupNavigation")]
    public virtual ICollection<DonXinHoTro> DonXinHoTros { get; set; } = new List<DonXinHoTro>();

    [InverseProperty("IdnguoiCanGiupNavigation")]
    public virtual ICollection<LuotTraoTang> LuotTraoTangs { get; set; } = new List<LuotTraoTang>();
}
