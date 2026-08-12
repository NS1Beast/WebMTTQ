using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Lưu danh sách quyền truy cập vào session dưới dạng danh sách "maModule:CoQuyenXem:CoQuyenThem:CoQuyenSua:CoQuyenXoa".
        /// </summary>
        public static void SaveQuyenToSession(ISession session, List<ModuleQuyenInfo> quyens, bool isAdmin, string? tenVaiTro)
        {
            var list = new List<string>();
            foreach (var q in quyens)
            {
                list.Add($"{q.MaModule}:{q.CoQuyenXem}:{q.CoQuyenThem}:{q.CoQuyenSua}:{q.CoQuyenXoa}");
            }
            session.SetString(SessionKey_Quyen, string.Join("|", list));
            session.SetString(SessionKey_IsAdmin, isAdmin ? "1" : "0");
            session.SetString(SessionKey_VaiTro, tenVaiTro ?? "");
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
    }
}