using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("SoDuQuyViNguoiNgheo")]
    public class SoDuQuyViNguoiNgheo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiền mặt")]
        public decimal TienMat { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiền gửi ngân hàng")]
        public decimal TienGuiNganHang { get; set; }

        public DateTime NgayCapNhat { get; set; }

        // Thuộc tính này tự động tính tổng, không lưu vào database
        [NotMapped]
        public decimal TongTonQuy => TienMat + TienGuiNganHang;
    }
}