using System.ComponentModel.DataAnnotations;

namespace WebMTTQ.Models
{
    public class Banner
    {
        [Key]
        public int IdBanner { get; set; }
        public string ? TieuDe { get; set; }
        public string ? HinhAnh { get; set; }
        public string ? LienKet { get; set; }
        public int ThuTu { get; set; }
        public bool TrangThai { get; set; }
    }
}