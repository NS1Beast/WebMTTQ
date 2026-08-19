using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebMTTQ.Controllers
{
    public class VanBanTaiLieuController : Controller
    {
        private readonly DataMTTQContext _context;

        public VanBanTaiLieuController(DataMTTQContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách ngoài trang chủ
        public async Task<IActionResult> Index(string keyword, int? chuyenMucId, int page = 1)
        {
            int pageSize = 15;

            IQueryable<VanBanTaiLieu> query = _context.VanBanTaiLieus
                .Include(v => v.IdchuyenMucNavigation);

            if (chuyenMucId.HasValue && chuyenMucId.Value > 0)
            {
                query = query.Where(v => v.IdchuyenMuc == chuyenMucId.Value);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(v => v.TenVanBan.ToLower().Contains(lowerKeyword)
                                      || (v.SoHieu != null && v.SoHieu.ToLower().Contains(lowerKeyword)));
            }

            query = query.OrderByDescending(v => v.NgayBanHanh).ThenByDescending(v => v.IdvanBan);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var data = await query
                .Select(v => new VanBanTaiLieu
                {
                    IdvanBan = v.IdvanBan,
                    SoHieu = v.SoHieu,
                    TenVanBan = v.TenVanBan,
                    CoQuanBanHanh = v.CoQuanBanHanh,
                    NgayBanHanh = v.NgayBanHanh,
                    LoaiTep = v.LoaiTep,
                    DungLuong = v.DungLuong,
                    IdchuyenMucNavigation = v.IdchuyenMucNavigation
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            ViewBag.ChuyenMucId = chuyenMucId;

            // Lấy danh sách chuyên mục VĂN BẢN TÀI LIỆU (không lấy chuyên mục tin tức)
            var listChuyenMuc = await _context.ChuyenMucs
                .Where(c => c.LoaiChuyenMuc == LoaiChuyenMucConstants.VanBanTaiLieu)
                .OrderBy(c => c.ThuTu)
                .ToListAsync();
            ViewBag.ChuyenMucs = new SelectList(listChuyenMuc, "IdchuyenMuc", "TenChuyenMuc", chuyenMucId);

            return View(data);
        }

        // Action xử lý tải tệp đính kèm
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.VanBanTaiLieus.FirstOrDefaultAsync(v => v.IdvanBan == id);

            if (document == null || document.TepDinhKem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tệp đính kèm hoặc văn bản đã bị xóa.";
                return RedirectToAction("Index");
            }

            string fileName = !string.IsNullOrEmpty(document.SoHieu)
                ? $"VanBan_{document.SoHieu.Replace("/", "_")}"
                : $"VanBan_{document.IdvanBan}";

            string extension = ".pdf";
            string contentType = "application/pdf";

            if (!string.IsNullOrEmpty(document.LoaiTep))
            {
                var loaiTepLower = document.LoaiTep.ToLower();
                if (loaiTepLower.Contains("doc") || loaiTepLower.Contains("word"))
                {
                    extension = ".docx";
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }
                else if (loaiTepLower.Contains("xls") || loaiTepLower.Contains("excel"))
                {
                    extension = ".xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
            }

            fileName += extension;

            return File(document.TepDinhKem, contentType, fileName);
        }
    }
}