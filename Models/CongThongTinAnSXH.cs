using System;
using System.Collections.Generic;

namespace WebMTTQ.Models
{
    // Model tổng hợp dữ liệu cho trang Cổng thông tin an sinh xã hội
    public class CongThongTinAnSXH
    {
        public ThongKeQuy ? ThongKe { get; set; }
        public List<ChiTietUngHo>? DanhSachUngHo { get; set; }
    }

    public class ThongKeQuy
    {
        public decimal TongGiaTriTiepNhan { get; set; }
        public int LuotUngHo { get; set; }
        public decimal TienMat { get; set; }
        public decimal TienGuiNganHang { get; set; }
        public decimal TongTonQuy { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class ChiTietUngHo
    {
        public string? DonViCaNhan { get; set; }
        public DateTime ThoiGian { get; set; }
        public decimal GiaTri { get; set; }
    }
}