using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

[Table("VaiTro")]
public partial class VaiTro
{
    [Key]
    [Column("IDVaiTro")]
    public int IdvaiTro { get; set; }

    [StringLength(100)]
    public string TenVaiTro { get; set; } = null!;

    /// <summary>
    /// Quyền truy cập được lưu dạng bitmask (tinyint):
    /// 1 = Xem, 2 = Thêm, 4 = Sửa, 8 = Xóa.
    /// Admin (toàn quyền) = 15 (1+2+4+8).
    /// </summary>
    public byte? QuyenHan { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayTao { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? NgayCapNhat { get; set; }

    [InverseProperty("IdvaiTroNavigation")]
    public virtual ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();

    [InverseProperty("IdVaiTroNavigation")]
    public virtual ICollection<VaiTroQuyen> VaiTroQuyens { get; set; } = new List<VaiTroQuyen>();
}
