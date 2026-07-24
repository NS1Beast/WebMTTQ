namespace WebMTTQ.Models
{
    public class CauHinhViewModel
    {
        // 1. Thông tin cơ quan
        public string ? TenCoQuan { get; set; }
        public string ? DiaChi { get; set; }
        public string ? SoDienThoai { get; set; }
        public string ? Email { get; set; }
        public string ? GioLamViec { get; set; }

        // 3. Mạng xã hội
        public string ? LinkFacebook { get; set; }
        public string ? LinkZalo { get; set; }

        // 5. Trạng thái bảo trì
        public bool BaoTriHeThong { get; set; }
    }
}