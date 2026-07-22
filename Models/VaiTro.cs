using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("VaiTro")]
public partial class VaiTro
{
    [Key]
    [Column("IDVaiTro")]
    public int IdvaiTro { get; set; }

    [StringLength(100)]
    public string TenVaiTro { get; set; } = null!;

    public string? QuyenHan { get; set; }

    public bool? DaXoa { get; set; }

    [InverseProperty("IdvaiTroNavigation")]
    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
