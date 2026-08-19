using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("SoDuQuy")]
    public class SoDuQuy
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "Loại quỹ")]
        public string? LoaiQuy { get; set; }

        [Display(Name = "Tiền mặt")]
        public decimal? TienMat { get; set; }

        [Display(Name = "Tiền gửi ngân hàng")]
        public decimal? TienGuiNganHang { get; set; }

        [Display(Name = "Tổng tồn quỹ")]
        public decimal? TongTonQuy { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }
    }
}