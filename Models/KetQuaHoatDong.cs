using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebMTTQ.Models
{
    [Table("KetQuaHoatDong")]
    public class KetQuaHoatDong
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Loại hoạt động")]
        public string? LoaiHoatDong { get; set; }

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