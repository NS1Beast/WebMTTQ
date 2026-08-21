using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebMTTQ.Models;

/// <summary>
/// Lưu trữ lượt truy cập website theo từng phiên (session).
/// </summary>
[Table("LuotTruyCap")]
public class LuotTruyCap
{
    [Key]
    [Column("IDLuotTruyCap")]
    public int IdLuotTruyCap { get; set; }

    /// <summary>
    /// Mã phiên (session) của người truy cập - dùng để xác định "đang truy cập".
    /// </summary>
    [Column("SessionId")]
    [StringLength(100)]
    [Unicode(false)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Đường dẫn trang được truy cập.
    /// </summary>
    [Column("DuongDan")]
    [StringLength(500)]
    public string? DuongDan { get; set; }

    /// <summary>
    /// Thời điểm truy cập đầu tiên của phiên (dùng cho thống kê hôm nay, tuần này, tổng).
    /// </summary>
    [Column("ThoiGianTruyCap", TypeName = "datetime")]
    public DateTime ThoiGianTruyCap { get; set; } = DateTime.Now;

    /// <summary>
    /// Thời điểm hoạt động gần nhất của phiên (dùng để xác định "đang truy cập").
    /// </summary>
    [Column("LanTruyCapCuoi", TypeName = "datetime")]
    public DateTime LanTruyCapCuoi { get; set; } = DateTime.Now;
}