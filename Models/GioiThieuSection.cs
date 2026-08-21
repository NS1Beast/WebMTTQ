using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    /// <summary>
    /// Một section nội dung trong trang giới thiệu.
    /// Admin có thể thêm/sửa/xóa, bật/tắt, sắp xếp thứ tự.
    /// </summary>
    [Table("GioiThieuSection")]
    public class GioiThieuSection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>Tiêu đề của section (bắt buộc)</summary>
        [Required(ErrorMessage = "Tiêu đề section không được để trống")]
        [StringLength(300, ErrorMessage = "Tiêu đề không được vượt quá 300 ký tự")]
        public string TieuDe { get; set; } = string.Empty;

        /// <summary>Nội dung HTML của section (rich text)</summary>
        public string? NoiDung { get; set; }

        /// <summary>Đường dẫn hình ảnh đại diện của section</summary>
        [StringLength(1000)]
        public string? HinhAnh { get; set; }

        /// <summary>Alt text cho hình ảnh</summary>
        [StringLength(500)]
        public string? AltText { get; set; }

        /// <summary>Loại section để phân loại hiển thị (vd: "lichsu", "sumenh", "thanhtuu", "noidung"...)</summary>
        [StringLength(50)]
        public string? LoaiSection { get; set; }

        /// <summary>Thứ tự hiển thị (số nhỏ hiển thị trước)</summary>
        public int ThuTu { get; set; } = 0;

        /// <summary>Trạng thái hiển thị (true = hiển thị, false = ẩn)</summary>
        public bool TrangThai { get; set; } = true;
    }
}