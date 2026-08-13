using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("DanhSachUngHoCuuTro")]
    public class DanhSachUngHoCuuTro
    {
        [Key] public int Id { get; set; }
        public string? TenNguoiUngHo { get; set; }
        public DateTime? NgayUngHo { get; set; }
        public decimal? SoTien { get; set; }
        public bool? HienThi { get; set; }
    }
}