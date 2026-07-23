using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("NguoiDanCanTroGiup")]
    public class NguoiDanCanTroGiup
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string ? HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string ? SoDienThoai { get; set; }

        public string ? MucDoUuTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string ? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung cần trợ giúp")]
        public string ? NoiDung { get; set; }

        public DateTime? NgayGui { get; set; }
        public string ?TrangThai { get; set; }
        public bool? DaXoa { get; set; }
    }
}