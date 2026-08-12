using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    public interface IQuyenTruyCapService
    {
        /// <summary>
        /// Lấy danh sách quyền truy cập của một người dùng dựa trên vai trò của họ.
        /// </summary>
        Task<List<ModuleQuyenInfo>> GetQuyenCuaNguoiDungAsync(int idnguoiDung);

        /// <summary>
        /// Lấy danh sách module mà người dùng có quyền (có CoQuyenXem = true).
        /// </summary>
        Task<List<string>> GetModulesDuocQuyenAsync(int idnguoiDung);

        /// <summary>
        /// Kiểm tra người dùng có quyền xem 1 module cụ thể hay không.
        /// </summary>
        Task<bool> CoQuyenXemAsync(int idnguoiDung, string maModule);

        /// <summary>
        /// Kiểm tra người dùng có quyền thêm trên 1 module.
        /// </summary>
        Task<bool> CoQuyenThemAsync(int idnguoiDung, string maModule);

        /// <summary>
        /// Kiểm tra người dùng có quyền sửa trên 1 module.
        /// </summary>
        Task<bool> CoQuyenSuaAsync(int idnguoiDung, string maModule);

        /// <summary>
        /// Kiểm tra người dùng có quyền xóa trên 1 module.
        /// </summary>
        Task<bool> CoQuyenXoaAsync(int idnguoiDung, string maModule);

        /// <summary>
        /// Lưu danh sách quyền cho 1 vai trò (lưu dạng bitmask tinyint trong VaiTro.QuyenHan).
        /// </summary>
        Task SaveQuyenChoVaiTroAsync(int idVaiTro, List<ModuleQuyenInfo> quyens);

        /// <summary>
        /// Lấy danh sách quyền của 1 vai trò.
        /// </summary>
        Task<List<ModuleQuyenInfo>> GetQuyenCuaVaiTroAsync(int idVaiTro);
    }
}