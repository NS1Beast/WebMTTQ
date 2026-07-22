using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("CauHinhHeThong")]
[Index("MaCauHinh", Name = "UQ__CauHinhH__F0685B7C0CDBF0F8", IsUnique = true)]
public partial class CauHinhHeThong
{
    [Key]
    [Column("IDCauHinh")]
    public int IdcauHinh { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string MaCauHinh { get; set; } = null!;

    public string? GiaTriCauHinh { get; set; }

    [StringLength(255)]
    public string? MoTa { get; set; }
}
