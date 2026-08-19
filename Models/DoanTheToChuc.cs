using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("DoanTheToChuc")]
public partial class DoanTheToChuc
{
    [Key]
    [Column("IDDonVi")]
    public int IddonVi { get; set; }

    [StringLength(255)]
    public string TenDonVi { get; set; } = null!;

    public byte[]? LogoDaiDien { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? MauSacHienThi { get; set; }

    [InverseProperty("IddonViNavigation")]
    public virtual ICollection<DiaDiemBanDo> DiaDiemBanDos { get; set; } = new List<DiaDiemBanDo>();
}
