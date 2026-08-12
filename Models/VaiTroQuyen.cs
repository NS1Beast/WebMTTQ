using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models;

[Table("VaiTroQuyen")]
public partial class VaiTroQuyen
{
    [Key]
    public int Id { get; set; }

    [Column("IDVaiTro")]
    public int IdVaiTro { get; set; }

    [StringLength(50)]
    public string MaModule { get; set; } = string.Empty;

    public bool CoQuyenXem { get; set; }
    public bool CoQuyenThem { get; set; }
    public bool CoQuyenSua { get; set; }
    public bool CoQuyenXoa { get; set; }

    [ForeignKey("IdVaiTro")]
    [InverseProperty("VaiTroQuyens")]
    public virtual VaiTro IdVaiTroNavigation { get; set; } = null!;
}