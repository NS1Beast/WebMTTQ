using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using WebMTTQ.Models;

namespace WebMTTQ.Controllers
{
    public class CongThongTinAnSXHController : Controller
    {
        // 1. Khai báo context
        private readonly DataMTTQContext _context;

        public CongThongTinAnSXHController(DataMTTQContext context)
        {
            _context = context;
        }
        // GET: /CongThongTinAnSXH/
        public async Task<IActionResult> Index()
        {
            // 2. Lấy danh sách thông tin ủng hộ từ Database
            // Lấy những tài khoản đang được kích hoạt (nếu bạn có cột Trạng thái)
            var danhSachUngHo = await _context.ThongTinNhanUngHos.ToListAsync();

            // 3. Truyền dữ liệu ra View bằng ViewBag (hoặc truyền trực tiếp vào Model)
            ViewBag.DanhSachUngHo = danhSachUngHo;

            return View();
        }
    }
}