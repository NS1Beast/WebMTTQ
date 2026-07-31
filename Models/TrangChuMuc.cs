using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models;

[Table("TrangChuMuc")]
public class TrangChuMuc
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(500)]
    public string TieuDe { get; set; } = "";

    [StringLength(100)]
    public string Loai { get; set; } = "tin-tuc"; // tin-tuc, hinh-anh, thong-tin, video, van-ban, lien-ket

    public string? NoiDung { get; set; } // HTML content or JSON data

    [StringLength(500)]
    public string? HinhAnh { get; set; }

    public bool TrangThai { get; set; } = true;

    public int ThuTu { get; set; } = 0;

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public DateTime? NgayCapNhat { get; set; }

    // Collection of news items belonging to this section
    public virtual ICollection<TrangChuTinTuc> TinTucs { get; set; } = new List<TrangChuTinTuc>();
}
