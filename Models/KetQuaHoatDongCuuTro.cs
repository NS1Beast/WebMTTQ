using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("KetQuaHoatDongCuuTro")]
    public class KetQuaHoatDongCuuTro
    {
        [Key] public int Id { get; set; }
        public int? Thang { get; set; }
        public int? Nam { get; set; }
        public string? DonViUngHo { get; set; }
        public string? PhanLoaiDonVi { get; set; }
        public string? NoiDung { get; set; }
        public int? SoLuongHo { get; set; }
        public decimal? KinhPhi { get; set; }
        public bool? TrangThai { get; set; }
    }
}