using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Index(nameof(Thang))] // Tạo Index cho tháng
    [Index(nameof(PhanLoaiDonVi))]
    [Table("KetQuaChamLo")]
 
    public class KetQuaChamLo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Thang { get; set; }

        public int Nam { get; set; } = 2026;

        [Required(ErrorMessage = "Vui lòng nhập tên đơn vị")]
        public string ? DonViUngHo { get; set; }

        [Required(ErrorMessage = "Vui lòng phân loại đơn vị")]
        public string ? PhanLoaiDonVi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string ? NoiDung { get; set; }

        [Required]
        public int SoLuongHo { get; set; }

        [Required]
        public decimal KinhPhi { get; set; }

        public DateTime NgayCapNhat { get; set; }
    }
}