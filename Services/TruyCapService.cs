using Microsoft.EntityFrameworkCore;
using WebMTTQ.Models;

namespace WebMTTQ.Services;

/// <summary>
/// Triển khai service thống kê truy cập website.
/// </summary>
public class TruyCapService : ITruyCapService
{
    private readonly DataMTTQContext _context;

    /// <summary>
    /// Khoảng thời gian (phút) để xác định một phiên vẫn "đang truy cập".
    /// </summary>
    private const int DangTruyCapTimeoutPhut = 15;

    public TruyCapService(DataMTTQContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task GhiNhanTruyCapAsync(string sessionId, string? duongDan)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        var now = DateTime.Now;

        // Tìm bản ghi của session này
        var existing = await _context.LuotTruyCaps
            .FirstOrDefaultAsync(x => x.SessionId == sessionId);

        if (existing != null)
        {
            // Session đã tồn tại -> chỉ cập nhật thời gian truy cập cuối
            existing.LanTruyCapCuoi = now;
            existing.DuongDan = duongDan;
        }
        else
        {
            // Session mới -> thêm bản ghi mới
            _context.LuotTruyCaps.Add(new LuotTruyCap
            {
                SessionId = sessionId,
                DuongDan = duongDan,
                ThoiGianTruyCap = now,
                LanTruyCapCuoi = now
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<ThongKeTruyCapResult> LayThongKeAsync()
    {
        var now = DateTime.Now;
        var startOfDay = now.Date;
        var startOfWeek = startOfDay.AddDays(-(int)now.DayOfWeek); // Thứ 2 là ngày đầu tuần
        var dangTruyCapThreshold = now.AddMinutes(-DangTruyCapTimeoutPhut);

        // Đang truy cập: các phiên có hoạt động trong 15 phút gần nhất
        var dangTruyCap = await _context.LuotTruyCaps
            .Where(x => x.LanTruyCapCuoi >= dangTruyCapThreshold)
            .Select(x => x.SessionId)
            .Distinct()
            .CountAsync();

        // Hôm nay: số phiên duy nhất truy cập hôm nay
        var homNay = await _context.LuotTruyCaps
            .Where(x => x.ThoiGianTruyCap >= startOfDay)
            .Select(x => x.SessionId)
            .Distinct()
            .CountAsync();

        // Tuần này: số phiên duy nhất truy cập từ đầu tuần
        var tuanNay = await _context.LuotTruyCaps
            .Where(x => x.ThoiGianTruyCap >= startOfWeek)
            .Select(x => x.SessionId)
            .Distinct()
            .CountAsync();

        // Tổng truy cập: tổng số phiên duy nhất
        var tongTruyCap = await _context.LuotTruyCaps
            .Select(x => x.SessionId)
            .Distinct()
            .CountAsync();

        return new ThongKeTruyCapResult
        {
            DangTruyCap = dangTruyCap,
            HomNay = homNay,
            TuanNay = tuanNay,
            TongTruyCap = tongTruyCap
        };
    }
}