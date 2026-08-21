using WebMTTQ.Models;

namespace WebMTTQ.Services;

/// <summary>
/// Kết quả thống kê truy cập.
/// </summary>
public class ThongKeTruyCapResult
{
    public int DangTruyCap { get; set; }
    public int HomNay { get; set; }
    public int TuanNay { get; set; }
    public int TongTruyCap { get; set; }
}

/// <summary>
/// Service quản lý thống kê truy cập website.
/// </summary>
public interface ITruyCapService
{
    /// <summary>
    /// Ghi nhận lượt truy cập theo session hiện tại.
    /// Nếu session đã tồn tại thì chỉ cập nhật thời gian truy cập cuối.
    /// </summary>
    Task GhiNhanTruyCapAsync(string sessionId, string? duongDan);

    /// <summary>
    /// Lấy thống kê truy cập: đang truy cập, hôm nay, tuần này, tổng truy cập.
    /// </summary>
    Task<ThongKeTruyCapResult> LayThongKeAsync();
}