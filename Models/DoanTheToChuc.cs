using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("DoanTheToChuc")]
    public class DoanTheToChuc
    {
        [Key]
        public int Id { get; set; }
        public string? HoTen { get; set; }
        public string? ChucVu { get; set; }
        public string? CoQuan { get; set; }
        public string? VaiTroBoSung { get; set; }
        public string? HinhAnh { get; set; }
        public string? MauSac { get; set; }
        public int? CapDo { get; set; }
        public int? ThuTu { get; set; }
        public bool? DaXoa { get; set; }
    }
}