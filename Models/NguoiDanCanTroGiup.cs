using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models
{
    [Table("NguoiDanCanTroGiup")]
    public class NguoiDanCanTroGiup
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 255 ký tự")]
        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Unicode(false)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(50)]
        [Display(Name = "Mức độ ưu tiên")]
        public string? MucDoUuTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Địa chỉ phải từ 5 đến 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung cần trợ giúp")]
        [StringLength(10000, MinimumLength = 10, ErrorMessage = "Nội dung phải từ 10 đến 10.000 ký tự")]
        [Display(Name = "Nội dung cần trợ giúp")]
        public string? NoiDung { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime? NgayGui { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; }
    }
}