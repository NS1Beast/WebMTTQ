using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models
{
    [Table("GioiThieuChung")]
    public class GioiThieuChung
    {
        [Key]
        public int Id { get; set; }
        public string? TieuDeChinh { get; set; }
        public string? ChuNghieng { get; set; }
        public string? TieuDePhu { get; set; }
        public string? Slogan { get; set; }
        public string? NoiDung { get; set; }
        public bool? TrangThai { get; set; }
    }
}