using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models;

[Table("TimelineSection")]
public class TimelineSection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Bật/Tắt toàn bộ section</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>Eyebrow text (ví dụ: "CÁC CÔNG TRÌNH SỐ")</summary>
    [StringLength(100)]
    public string? Eyebrow { get; set; }

    /// <summary>Title của section (ví dụ: "Hành trình chuyển đổi số phường Tân Định")</summary>
    [StringLength(300)]
    public string? Title { get; set; }

    public virtual ICollection<TimelineItem> Items { get; set; } = new List<TimelineItem>();
}