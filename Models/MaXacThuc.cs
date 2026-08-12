using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("MaXacThuc")]
public partial class MaXacThuc
{
    [Key]
    [Column("IDMaXacThuc")]
    public int IdmaXacThuc { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string MaOtp { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime NgayTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime HanHet { get; set; }

    public bool DaSuDung { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? DiaChiIp { get; set; }
}