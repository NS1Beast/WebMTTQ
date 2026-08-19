using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

/// <summary>
/// Hằng số loại chuyên mục - dùng để phân biệt chuyên mục của Tin tức và Văn bản tài liệu
/// </summary>
public static class LoaiChuyenMucConstants
{
    /// <summary>Chuyên mục dùng cho Tin tức (Bảng BaiViet)</summary>
    public const string TinTuc = "TinTuc";

    /// <summary>Chuyên mục dùng cho Văn bản tài liệu (Bảng VanBanTaiLieu)</summary>
    public const string VanBanTaiLieu = "VanBanTaiLieu";
}

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

    /// <summary>
    /// Loại chuyên mục: 
    /// - "TinTuc" = Chuyên mục cho Tin tức (BaiViet)
    /// - "VanBanTaiLieu" = Chuyên mục cho Văn bản tài liệu (VanBanTaiLieu)
    /// </summary>
    [StringLength(50)]
    public string? LoaiChuyenMuc { get; set; }

    public int? ThuTu { get; set; }

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