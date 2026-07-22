using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("BaiViet")]
[Index("DaXoa", Name = "IDX_BaiViet_DaXoa")]
[Index("DuongDan", Name = "IDX_BaiViet_DuongDan")]
public partial class BaiViet
{
    [Key]
    [Column("IDBaiViet")]
    public int IdbaiViet { get; set; }

    [StringLength(500)]
    public string TieuDe { get; set; } = null!;

    [StringLength(500)]
    [Unicode(false)]
    public string DuongDan { get; set; } = null!;

    public string? TomTat { get; set; }

    public string? NoiDung { get; set; }

    public byte[]? AnhDaiDien { get; set; }

    [Column("IDChuyenMuc")]
    public int? IdchuyenMuc { get; set; }

    [Column("IDNguoiDung")]
    public int? IdnguoiDung { get; set; }

    public int? LuotXem { get; set; }

    public bool? LaTinNoiBat { get; set; }

    [StringLength(50)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayXuatBan { get; set; }

    public bool? DaXoa { get; set; }

    [ForeignKey("IdchuyenMuc")]
    [InverseProperty("BaiViets")]
    public virtual ChuyenMuc? IdchuyenMucNavigation { get; set; }

    [ForeignKey("IdnguoiDung")]
    [InverseProperty("BaiViets")]
    public virtual NguoiDung? IdnguoiDungNavigation { get; set; }
}
