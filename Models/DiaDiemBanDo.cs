using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models
{
    [Table("DiaDiemBanDo")]
    public partial class DiaDiemBanDo
    {
        [Key]
        [Column("IDDiaDiem")]
        public int IddiaDiem { get; set; }

        [StringLength(500)]
        public string TenDiaDiem { get; set; } = null!;

        [StringLength(50)]
        public string PhanLoaiBanDo { get; set; } = null!; // VD: Đoàn Thanh Niên, Hội LHPN...

        [Column(TypeName = "decimal(10, 8)")]
        public decimal ViDo { get; set; } // Vĩ độ (Latitude)

        [Column(TypeName = "decimal(10, 8)")]
        public decimal KinhDo { get; set; } // Kinh độ (Longitude)

        [StringLength(500)]
        public string? DiaChi { get; set; }

        public string? MoTaChiTiet { get; set; } // Nội dung: Trao sổ tiết kiệm...

        // ---> THÊM TRƯỜNG NÀY ĐỂ HIỂN THỊ NGÀY THÁNG TRÊN GIAO DIỆN <---
        public DateTime? NgayThucHien { get; set; }

        [StringLength(255)]
        public string? ThongTinLienHe { get; set; }

        [Column("IDDonVi")]
        public int? IddonVi { get; set; }

        public byte[]? HinhAnhThucTe { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        [ForeignKey("IddonVi")]
        [InverseProperty("DiaDiemBanDos")]
        public virtual DoanTheToChuc? IddonViNavigation { get; set; }
    }
}