using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Implementation of IQuyenTruyCapService.
    /// Quyền truy cập chi tiết theo module được lưu trong bảng VaiTroQuyen.
    /// VaiTro.QuyenHan (bitmask tinyint) vẫn được giữ để tương thích:
    /// - Admin (toàn quyền) = 15 (1+2+4+8)
    /// - Các vai trò khác: bitmask tổng hợp từ các quyền đã chọn (dùng cho kiểm tra nhanh)
    /// </summary>
    public class QuyenTruyCapService : IQuyenTruyCapService
    {
        private readonly DataMTTQContext _context;

        public QuyenTruyCapService(DataMTTQContext context)
        {
            _context = context;
        }

        public async Task<List<ModuleQuyenInfo>> GetQuyenCuaNguoiDungAsync(int idnguoiDung)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.IdvaiTroNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdnguoiDung == idnguoiDung);

            if (user?.IdvaiTroNavigation == null)
                return new List<ModuleQuyenInfo>();

            return await GetQuyenCuaVaiTroAsync(user.IdvaiTroNavigation.IdvaiTro);
        }

        public async Task<List<string>> GetModulesDuocQuyenAsync(int idnguoiDung)
        {
            var quyens = await GetQuyenCuaNguoiDungAsync(idnguoiDung);
            return quyens.Where(q => q.CoQuyenXem).Select(q => q.MaModule).ToList();
        }

        public async Task<bool> CoQuyenXemAsync(int idnguoiDung, string maModule)
        {
            var quyens = await GetQuyenCuaNguoiDungAsync(idnguoiDung);
            return quyens.Any(q => q.MaModule == maModule && q.CoQuyenXem);
        }

        public async Task<bool> CoQuyenThemAsync(int idnguoiDung, string maModule)
        {
            var quyens = await GetQuyenCuaNguoiDungAsync(idnguoiDung);
            return quyens.Any(q => q.MaModule == maModule && q.CoQuyenThem);
        }

        public async Task<bool> CoQuyenSuaAsync(int idnguoiDung, string maModule)
        {
            var quyens = await GetQuyenCuaNguoiDungAsync(idnguoiDung);
            return quyens.Any(q => q.MaModule == maModule && q.CoQuyenSua);
        }

        public async Task<bool> CoQuyenXoaAsync(int idnguoiDung, string maModule)
        {
            var quyens = await GetQuyenCuaNguoiDungAsync(idnguoiDung);
            return quyens.Any(q => q.MaModule == maModule && q.CoQuyenXoa);
        }

        public async Task SaveQuyenChoVaiTroAsync(int idVaiTro, List<ModuleQuyenInfo> quyens)
        {
            var vaiTro = await _context.VaiTros.FindAsync(idVaiTro);
            if (vaiTro == null) return;

            // Admin role toàn quyền sẽ có bitmask 15
            if (QuyenHelper.IsAdminVaiTro(vaiTro.TenVaiTro))
            {
                vaiTro.QuyenHan = QuyenBitmask.ToanQuyen;
            }
            else
            {
                // Không dùng bitmask 15 cho vai trò không phải admin
                // vì điều này sẽ khiến GetQuyenCuaVaiTroAsync trả về toàn quyền tất cả modules.
                // Chỉ lưu bitmask tổng hợp để tương thích, nhưng không bao giờ = 15.
                var bitmask = ConvertModulesToQuyenHan(quyens);
                vaiTro.QuyenHan = bitmask == QuyenBitmask.ToanQuyen ? (byte)0 : bitmask;
            }
            vaiTro.NgayCapNhat = DateTime.Now;

            // Xóa quyền cũ và lưu quyền mới chi tiết theo module
            var oldQuyens = await _context.VaiTroQuyens.Where(q => q.IdVaiTro == idVaiTro).ToListAsync();
            if (oldQuyens.Count > 0)
            {
                _context.VaiTroQuyens.RemoveRange(oldQuyens);
            }

            if (quyens != null)
            {
                foreach (var q in quyens)
                {
                    if (q.CoQuyenXem || q.CoQuyenThem || q.CoQuyenSua || q.CoQuyenXoa)
                    {
                        _context.VaiTroQuyens.Add(new VaiTroQuyen
                        {
                            IdVaiTro = idVaiTro,
                            MaModule = q.MaModule,
                            CoQuyenXem = q.CoQuyenXem,
                            CoQuyenThem = q.CoQuyenThem,
                            CoQuyenSua = q.CoQuyenSua,
                            CoQuyenXoa = q.CoQuyenXoa
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ModuleQuyenInfo>> GetQuyenCuaVaiTroAsync(int idVaiTro)
        {
            var vaiTro = await _context.VaiTros.AsNoTracking().FirstOrDefaultAsync(v => v.IdvaiTro == idVaiTro);
            if (vaiTro == null) return new List<ModuleQuyenInfo>();

            // Chỉ Admin role mới được toàn quyền tất cả modules.
            // KHÔNG dùng QuyenHan == 15 làm điều kiện vì vai trò thường
            // có thể chọn đủ 4 quyền cho 1 module -> bitmask = 15 nhưng
            // không phải là toàn quyền tất cả modules.
            if (QuyenHelper.IsAdminVaiTro(vaiTro.TenVaiTro))
            {
                return ConvertQuyenHanToModules(QuyenBitmask.ToanQuyen);
            }

            // Đọc quyền chi tiết theo module từ bảng VaiTroQuyen
            var quyenRecords = await _context.VaiTroQuyens
                .AsNoTracking()
                .Where(q => q.IdVaiTro == idVaiTro)
                .ToListAsync();

            if (quyenRecords.Count > 0)
            {
                var result = new List<ModuleQuyenInfo>();
                foreach (var q in quyenRecords)
                {
                    result.Add(new ModuleQuyenInfo
                    {
                        MaModule = q.MaModule,
                        TenModule = ModuleQuyen.GetAllModules().FirstOrDefault(m => m.MaModule == q.MaModule)?.TenModule ?? q.MaModule,
                        CoQuyenXem = q.CoQuyenXem,
                        CoQuyenThem = q.CoQuyenThem,
                        CoQuyenSua = q.CoQuyenSua,
                        CoQuyenXoa = q.CoQuyenXoa
                    });
                }
                return result;
            }

            // Fallback: nếu chưa có bản ghi chi tiết, dùng bitmask cũ
            // Lưu ý: KHÔNG trả về toàn quyền tất cả modules nếu QuyenHan = 15
            // vì vai trò thường có thể chọn đủ 4 quyền cho 1 module.
            // Chỉ Admin role mới được toàn quyền (đã xử lý ở trên).
            if (vaiTro.QuyenHan == QuyenBitmask.ToanQuyen)
            {
                return new List<ModuleQuyenInfo>();
            }
            return ConvertQuyenHanToModules(vaiTro.QuyenHan);
        }

        /// <summary>
        /// Convert bitmask quyenHan thành danh sách quyền module.
        /// Nếu quyenHan = ToanQuyen (15) hoặc role là Admin, trả về toàn bộ module với toàn quyền.
        /// </summary>
        private List<ModuleQuyenInfo> ConvertQuyenHanToModules(byte? quyenHan)
        {
            var result = new List<ModuleQuyenInfo>();
            var allModules = ModuleQuyen.GetAllModules();

            // Nếu quyenHan = toàn quyền (15) thì cấp toàn quyền tất cả modules
            if (quyenHan == QuyenBitmask.ToanQuyen)
            {
                foreach (var module in allModules)
                {
                    result.Add(new ModuleQuyenInfo
                    {
                        MaModule = module.MaModule,
                        TenModule = module.TenModule,
                        CoQuyenXem = true,
                        CoQuyenThem = true,
                        CoQuyenSua = true,
                        CoQuyenXoa = true
                    });
                }
                return result;
            }

            // Nếu quyenHan > 0, cấp quyền theo bitmask cho tất cả modules
            var coXem = QuyenBitmask.CoQuyenXem(quyenHan);
            var coThem = QuyenBitmask.CoQuyenThem(quyenHan);
            var coSua = QuyenBitmask.CoQuyenSua(quyenHan);
            var coXoa = QuyenBitmask.CoQuyenXoa(quyenHan);

            // Nếu không có quyền nào thì trả về rỗng
            if (!coXem && !coThem && !coSua && !coXoa)
            {
                return result;
            }

            // Cho tất cả module cùng mức quyền dựa trên bitmask
            foreach (var module in allModules)
            {
                result.Add(new ModuleQuyenInfo
                {
                    MaModule = module.MaModule,
                    TenModule = module.TenModule,
                    CoQuyenXem = coXem,
                    CoQuyenThem = coThem,
                    CoQuyenSua = coSua,
                    CoQuyenXoa = coXoa
                });
            }

            return result;
        }

        /// <summary>
        /// Convert danh sách quyền module thành bitmask tổng hợp.
        /// Bitmask là tổng hợp của tất cả quyền được chọn trên các module.
        /// </summary>
        private byte? ConvertModulesToQuyenHan(List<ModuleQuyenInfo> quyens)
        {
            if (quyens == null || quyens.Count == 0) return 0;

            byte result = 0;
            foreach (var q in quyens)
            {
                if (q.CoQuyenXem) result |= QuyenBitmask.Xem;
                if (q.CoQuyenThem) result |= QuyenBitmask.Them;
                if (q.CoQuyenSua) result |= QuyenBitmask.Sua;
                if (q.CoQuyenXoa) result |= QuyenBitmask.Xoa;
            }
            return result;
        }
    }
}