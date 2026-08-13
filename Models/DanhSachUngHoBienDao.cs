using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace WebMTTQ.Models
{
    [Table("DanhSachUngHoBienDao")]
    public class DanhSachUngHoBienDao
    {
        [Key]
        public int Id { get; set; }
        public string? TenNguoiUngHo { get; set; }
        public DateTime? NgayUngHo { get; set; }
        public decimal? SoTien { get; set; }
        public bool? HienThi { get; set; }
    }
}
