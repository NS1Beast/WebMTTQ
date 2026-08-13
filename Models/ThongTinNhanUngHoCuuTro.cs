using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("ThongTinNhanUngHoCuuTro")]
    public class ThongTinNhanUngHoCuuTro
    {
        [Key] public int Id { get; set; }
        public string? TenNganHang { get; set; }
        public string? ChiNhanh { get; set; }
        public string? TenTaiKhoan { get; set; }
        public string? SoTaiKhoan { get; set; }
        public string? DonViThuHuong { get; set; }
        public string? NoiDungChuyenKhoan { get; set; }
        public string? QrCodeUrl { get; set; }
        public bool? TrangThai { get; set; }
    }
}