using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("VanBanTaiLieu")]
[Index("DaXoa", Name = "IDX_VanBan_DaXoa")]
public partial class VanBanTaiLieu
{
    [Key]
    [Column("IDVanBan")]
    public int IdvanBan { get; set; }

    [StringLength(100)]
    public string? SoHieu { get; set; }

    [StringLength(500)]
    public string TenVanBan { get; set; } = null!;

    [StringLength(255)]
    public string? CoQuanBanHanh { get; set; }

    public byte[]? TepDinhKem { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? LoaiTep { get; set; }

    public double? DungLuong { get; set; }

    [Column("IDChuyenMuc")]
    public int? IdchuyenMuc { get; set; }

    public DateOnly? NgayBanHanh { get; set; }

    public bool? DaXoa { get; set; }

    [ForeignKey("IdchuyenMuc")]
    [InverseProperty("VanBanTaiLieus")]
    public virtual ChuyenMuc? IdchuyenMucNavigation { get; set; }
}
