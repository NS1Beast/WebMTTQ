using System.ComponentModel.DataAnnotations;

namespace WebMTTQ.Models
{
    public class ThongTinNhanUngHo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên ngân hàng")]
        [Display(Name = "Tên ngân hàng")]
        public string ? TenNgânHang { get; set; } // VD: BIDV

        [Display(Name = "Chi nhánh")]
        public string ? ChiNhanh { get; set; } // VD: Chi nhánh Củ Chi

        [Display(Name = "Link hoặc Ảnh QR")]
        public string ?  QrCodeUrl { get; set; } // Link ảnh mã QR

        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [Display(Name = "Tên tài khoản")]
        public string ? TenTaiKhoan { get; set; } // VD: QUY VI NGUOI NGHEO...

        [Required(ErrorMessage = "Vui lòng nhập số tài khoản")]
        [Display(Name = "Số tài khoản")]
        public string ? SoTaiKhoan { get; set; } // VD: 8675789789

        [Display(Name = "Đơn vị thụ hưởng")]
        public string ? DonViThuHuong { get; set; }

        [Display(Name = "Nội dung chuyển khoản mẫu")]
        public string ? NoiDungChuyenKhoan { get; set; }

        [Display(Name = "Trạng thái hiển thị")]
        public bool TrangThai { get; set; } // Chọn tài khoản nào đang hoạt động chính
    }
}