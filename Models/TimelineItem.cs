using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMTTQ.Models;

[Table("TimelineItem")]
public class TimelineItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("IdTimelineSection")]
    public int IdTimelineSection { get; set; }

    /// <summary>Nhãn thời gian (tự do, ví dụ: "12/8/2025" hoặc "Dự kiến 7/2026")</summary>
    [Required]
    [StringLength(50, ErrorMessage = "TimeLabel tối đa 50 ký tự")]
    public string TimeLabel { get; set; } = "";

    /// <summary>Tiêu đề item</summary>
    [Required]
    [StringLength(300, ErrorMessage = "Tiêu đề tối đa 300 ký tự")]
    public string Title { get; set; } = "";

    /// <summary>Mô tả dạng plain text, tối đa 250 ký tự</summary>
    [StringLength(250, ErrorMessage = "Mô tả tối đa 250 ký tự")]
    public string? Description { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    [ForeignKey("IdTimelineSection")]
    public virtual TimelineSection? TimelineSection { get; set; }
}