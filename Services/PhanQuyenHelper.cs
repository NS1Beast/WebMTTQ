using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Helper class để lưu trữ và đọc thông tin quyền truy cập trong Session.
    /// </summary>
    public static class PhanQuyenHelper
    {
        public const string SessionKey_Quyen = "UserQuyen";
        public const string SessionKey_IsAdmin = "UserIsAdmin";
        public const string SessionKey_VaiTro = "AdminVaiTro";
        public const string SessionKey_VaiTroId = "AdminVaiTroId";
        public const string SessionKey_PhienBan = "AdminRoleVersion";

        /// <summary>
        /// Lưu danh sách quyền truy cập vào session dưới dạng danh sách "maModule:CoQuyenXem:CoQuyenThem:CoQuyenSua:CoQuyenXoa".
        /// idVaiTro là ID vai trò hiện tại của user; phienBan là số chữ ký (dùng VaiTro.NgayCapNhat.Ticks) 
        /// để phát hiện role/permission thay đổi.
        /// </summary>
        public static void SaveQuyenToSession(ISession session, List<ModuleQuyenInfo> quyens, bool isAdmin, string? tenVaiTro, int idVaiTro = 0, long phienBan = 0)
        {
            var list = new List<string>();
            foreach (var q in quyens)
            {
                list.Add($"{q.MaModule}:{q.CoQuyenXem}:{q.CoQuyenThem}:{q.CoQuyenSua}:{q.CoQuyenXoa}");
            }
            session.SetString(SessionKey_Quyen, string.Join("|", list));
            session.SetString(SessionKey_IsAdmin, isAdmin ? "1" : "0");
            session.SetString(SessionKey_VaiTro, tenVaiTro ?? "");
            session.SetInt32(SessionKey_VaiTroId, idVaiTro);
            session.SetString(SessionKey_PhienBan, phienBan.ToString());
        }

        /// <summary>
        /// Kiểm tra người dùng hiện tại có quyền xem module hay không.
        /// Admin luôn có quyền.
        /// </summary>
        public static bool CoQuyenXem(ISession session, string maModule)
        {
            if (IsAdmin(session)) return true;
            return GetModulePermission(session, maModule, 0);
        }

        /// <summary>
        /// Kiểm tra người dùng hiện tại có quyền thêm trên module hay không.
        /// Admin luôn có quyền.
        /// </summary>
        public static bool CoQuyenThem(ISession session, string maModule)
        {
            if (IsAdmin(session)) return true;
            return GetModulePermission(session, maModule, 1);
        }

        public static bool CoQuyenSua(ISession session, string maModule)
        {
            if (IsAdmin(session)) return true;
            return GetModulePermission(session, maModule, 2);
        }

        public static bool CoQuyenXoa(ISession session, string maModule)
        {
            if (IsAdmin(session)) return true;
            return GetModulePermission(session, maModule, 3);
        }

        /// <summary>
        /// Kiểm tra người dùng hiện tại có phải là Admin hay không.
        /// </summary>
        public static bool IsAdmin(ISession session)
        {
            return session.GetString(SessionKey_IsAdmin) == "1";
        }

        /// <summary>
        /// Lấy danh sách module mà người dùng có quyền xem (dùng cho menu).
        /// </summary>
        public static List<string> GetModulesDuocQuyen(ISession session)
        {
            var result = new List<string>();
            var raw = session.GetString(SessionKey_Quyen);
            if (string.IsNullOrEmpty(raw)) return result;

            foreach (var item in raw.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split(':');
                if (parts.Length >= 2 && parts[1] == "True")
                {
                    result.Add(parts[0]);
                }
            }
            return result;
        }

        /// <summary>
        /// Lấy thông tin quyền của 1 module. index: 0=Xem, 1=Them, 2=Sua, 3=Xoa.
        /// </summary>
        private static bool GetModulePermission(ISession session, string maModule, int index)
        {
            var raw = session.GetString(SessionKey_Quyen);
            if (string.IsNullOrEmpty(raw)) return false;

            foreach (var item in raw.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = item.Split(':');
                if (parts.Length >= 5 && parts[0] == maModule)
                {
                    bool.TryParse(parts[index + 1], out var result);
                    return result;
                }
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra nếu role hoặc permission của user trong database đã thay đổi so với session lưu.
        /// Nếu có, reload permission từ database và cập nhật lại session.
        /// 
        /// Chỉ thực hiện một query duy nhất mỗi request để kiểm tra roleId và NgayCapNhat của role hiện tại.
        /// Nếu roleId và version giống với session thì giữ nguyên session (không query thêm).
        /// </summary>
        public static async Task RefreshSessionQuyenIfNeededAsync(HttpContext context)
        {
            var session = context.Session;

            // Tránh query nhiều lần trong cùng 1 request
            if (context.Items.ContainsKey("PermissionRefreshed")) return;

            try
            {
                // Đánh dấu đã xử lý trong request này
                context.Items["PermissionRefreshed"] = true;

                // Chỉ xử lý khi user đã đăng nhập
                if (session.GetString("AdminLoggedIn") != "true") return;

                var userIdStr = session.GetString("AdminUserId");
                if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId) || userId <= 0)
                {
                    return;
                }

                var db = context.RequestServices.GetRequiredService<DataMTTQContext>();
                var quyenService = context.RequestServices.GetRequiredService<IQuyenTruyCapService>();

                // Query nhẹ: chỉ lấy roleId, status, NgayCapNhat, NgayTao, và count VaiTroQuyen
                // cho version của role hiện tại
                var current = await db.NguoiDungs
                    .AsNoTracking()
                    .Where(u => u.IdnguoiDung == userId)
                    .Select(u => new
                    {
                        u.IdvaiTro,
                        u.TrangThai,
                        TenVaiTro = u.IdvaiTroNavigation != null ? u.IdvaiTroNavigation.TenVaiTro : null,
                        NgayCapNhat = u.IdvaiTroNavigation != null ? u.IdvaiTroNavigation.NgayCapNhat : (DateTime?)null,
                        NgayTao = u.IdvaiTroNavigation != null ? u.IdvaiTroNavigation.NgayTao : (DateTime?)null,
                        QuyenCount = u.IdvaiTroNavigation != null ? u.IdvaiTroNavigation.VaiTroQuyens.Count() : 0
                    })
                    .FirstOrDefaultAsync();

                // User bị xóa, hoặc bị khóa → đăng xuất để security
                if (current == null || current.TrangThai == "Khoa" || current.TrangThai == "BiXoa")
                {
                    session.Clear();
                    return;
                }

                int? sessionRoleId = session.GetInt32(SessionKey_VaiTroId);
                long sessionVersion = 0;
                long.TryParse(session.GetString(SessionKey_PhienBan), out sessionVersion);

                // Version tối ưu:
                // - Nếu NgayCapNhat có giá trị → dùng Ticks.
                // - Nếu null → fallback bằng NgayTao (đã có từ seed/create).
                // - Nếu cả hai đều null → dùng count VaiTroQuyen+1 (độc nhất cho số quyền hiện tại)
                //   + version seed constant. Khi role được update (PermissionVersion will set NgayCapNhat),
                //   version sẽ khác → phát hiện thay đổi.
                long currentVersion = 0;
                if (current.NgayCapNhat.HasValue)
                {
                    currentVersion = current.NgayCapNhat.Value.Ticks;
                }
                else if (current.NgayTao.HasValue)
                {
                    currentVersion = current.NgayTao.Value.Ticks;
                }
                else
                {
                    // Legacy role: NgayCapNhat = NgayTạo = null → dùng count quyền làm version
                    // để nếu ai thêm/xóa VaiTroQuyen trực tiếp thì version sẽ thay đổi.
                    long roleIdFactor = (long)(current.IdvaiTro ?? 0) * 1000000;
                    currentVersion = roleIdFactor + current.QuyenCount + 1;
                }

                if (sessionRoleId.HasValue && (current.IdvaiTro ?? 0) == sessionRoleId.Value && currentVersion == sessionVersion)
                {
                    return;
                }

                // Role thay đổi hoặc permission của role đổi → reload toàn bộ quyền
                var quyens = await quyenService.GetQuyenCuaNguoiDungAsync(userId);
                var isAdmin = QuyenHelper.IsAdminVaiTro(current.TenVaiTro);

                // Cập nhật session với version đã chuẩn hóa
                session.SetString("AdminVaiTro", current.TenVaiTro ?? "");
                SaveQuyenToSession(session, quyens, isAdmin, current.TenVaiTro, current.IdvaiTro ?? 0, currentVersion);
            }
            catch (Exception)
            {
                // KHÔNG thất bại: nếu query DB lỗi thì vẫn sử dụng session cũ.
                // Vẫn an toàn vì nếu user bị hạ quyền, authorization check sẽ vẫn
                // dựa trên session cũ và bị chặn ở controller khi permission không đúng.
            }
        }

        /// <summary>
        /// Đồng bộ và cập nhật session permission mới nhất sau khi Admin thay đổi role/user.
        /// </summary>
        public static void InvalidSessionQuyen(ISession session)
        {
            // Xóa version để force reload permission ở request tiếp theo
            session.Remove(SessionKey_PhienBan);
        }
    }
}