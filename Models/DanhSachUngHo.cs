using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("DanhSachUngHo")]
    public class DanhSachUngHo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đơn vị/cá nhân")]
        [StringLength(255)]
        [Display(Name = "Đơn vị / Cá nhân")]
        public string ? TenNguoiUngHo { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        [DataType(DataType.Date)]
        [Display(Name = "Thời gian")]
        public DateTime NgayUngHo { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Column(TypeName = "decimal(18,0)")] // Dùng kiểu decimal để lưu tiền tệ chính xác
        [Display(Name = "Giá trị ủng hộ (VNĐ)")]
        public decimal SoTien { get; set; }
    }
}