using WebMTTQ.Models;

namespace WebMTTQ.Services
{
    /// <summary>
    /// Helper để kiểm tra trạng thái bảo trì của từng trang chức năng.
    /// </summary>
    public static class MaintenanceHelper
    {
        /// <summary>
        /// Kiểm tra xem trang hiện tại có đang bảo trì hay không.
        /// Nếu BaoTriToanBo = true thì tất cả các trang đều bị bảo trì.
        /// </summary>
        public static async Task<bool> IsPageUnderMaintenanceAsync(ISystemSettingsService settings, string pageKey)
        {
            // Nếu bảo trì toàn bộ thì tất cả các trang đều bị khóa
            if (await settings.GetBooleanAsync("BaoTriToanBo"))
                return true;

            // Kiểm tra bảo trì cho trang cụ thể
            return await settings.GetBooleanAsync(pageKey);
        }

        /// <summary>
        /// Kiểm tra xem trang chủ có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsHomeUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriTrangChu");
        }

        /// <summary>
        /// Kiểm tra xem trang tin tức có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsNewsUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriTinTuc");
        }

        /// <summary>
        /// Kiểm tra xem trang văn bản tài liệu có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsVanBanTaiLieuUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriVanBanTaiLieu");
        }

        /// <summary>
        /// Kiểm tra xem trang giới thiệu có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsGioiThieuUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriGioiThieu");
        }

        /// <summary>
        /// Kiểm tra xem trang góp ý có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsGopYUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriGopY");
        }

        /// <summary>
        /// Kiểm tra xem trang an sinh xã hội có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsAnSinhXaHoiUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriAnSinhXaHoi");
        }

        /// <summary>
        /// Kiểm tra xem trang quỹ cứu trợ có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsQuyCuuTroUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriQuyCuuTro");
        }

        /// <summary>
        /// Kiểm tra xem trang quỹ biển đảo có đang bảo trì hay không.
        /// </summary>
        public static async Task<bool> IsQuyBienDaoUnderMaintenanceAsync(ISystemSettingsService settings)
        {
            return await IsPageUnderMaintenanceAsync(settings, "BaoTriQuyBienDao");
        }
    }
}