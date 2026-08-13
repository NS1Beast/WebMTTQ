using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("SoDuQuyCuuTro")]
    public class SoDuQuyCuuTro
    {
        [Key] public int Id { get; set; }
        public decimal? TienMat { get; set; }
        public decimal? TienGuiNganHang { get; set; }
        public decimal? TongTonQuy { get; set; }
        public DateTime? NgayCapNhat { get; set; }
    }
}