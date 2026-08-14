using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("ChuyenMuc")]
[Index("DuongDan", Name = "IDX_ChuyenMuc_DuongDan")]
public partial class ChuyenMuc
{
    [Key]
    [Column("IDChuyenMuc")]
    public int IdchuyenMuc { get; set; }

    [StringLength(255)]
    public string TenChuyenMuc { get; set; } = null!;

    [StringLength(255)]
    [Unicode(false)]
    public string DuongDan { get; set; } = null!;

    [Column("IDChuyenMucCha")]
    public int? IdchuyenMucCha { get; set; }

    [StringLength(50)]
    public string? LoaiChuyenMuc { get; set; }

    public int? ThuTu { get; set; }

    public bool? DaXoa { get; set; }

    /// <summary>
    /// Trạng thái hiển thị trên dropdown menu (tối đa 10 chuyên mục hiển thị cùng lúc).
    /// true = Hiển thị, false = Ẩn.
    /// </summary>
    public bool? HienThi { get; set; } = true;

    [InverseProperty("IdchuyenMucNavigation")]
    public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();

    [ForeignKey("IdchuyenMucCha")]
    [InverseProperty("InverseIdchuyenMucChaNavigation")]
    public virtual ChuyenMuc? IdchuyenMucChaNavigation { get; set; }

    [InverseProperty("IdchuyenMucChaNavigation")]
    public virtual ICollection<ChuyenMuc> InverseIdchuyenMucChaNavigation { get; set; } = new List<ChuyenMuc>();

    [InverseProperty("IdchuyenMucNavigation")]
    public virtual ICollection<VanBanTaiLieu> VanBanTaiLieus { get; set; } = new List<VanBanTaiLieu>();
}
