using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("Banner")]
    public class Banner
    {
        [Key]
        public int IdBanner { get; set; }

        [StringLength(1000)]
        public string? HinhAnh { get; set; }

        [StringLength(1000)]
        public string? LienKet { get; set; }

        public int ThuTu { get; set; }

        public bool TrangThai { get; set; } = true;

        /// <summary>Hiệu ứng chuyển: slide, fade, zoom</summary>
        [StringLength(50)]
        public string? HieuUng { get; set; } = "slide";

        /// <summary>Tốc độ chuyển (ms)</summary>
        public int TocDo { get; set; } = 600;

        /// <summary>Thời gian dừng giữa các slide (ms)</summary>
        public int ThoiGianDung { get; set; } = 5000;

        /// <summary>Màu nền hiển thị khi ảnh chưa tải xong</summary>
        [StringLength(50)]
        public string? MauNen { get; set; } = "#1a1a2e";
    }
}