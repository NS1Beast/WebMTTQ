using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models;

[Table("TrangChuTinTuc")]
public class TrangChuTinTuc
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("IdTrangChuMuc")]
    public int IdTrangChuMuc { get; set; }

    [StringLength(500)]
    public string TieuDe { get; set; } = "";

    public string? TomTat { get; set; }

    [StringLength(1000)]
    public string? HinhAnh { get; set; }

    [StringLength(1000)]
    public string? LienKet { get; set; }

    public int ThuTu { get; set; } = 0;

    public bool TrangThai { get; set; } = true;

    public DateTime NgayTao { get; set; } = DateTime.Now;

    [ForeignKey("IdTrangChuMuc")]
    public virtual TrangChuMuc? TrangChuMuc { get; set; }
}