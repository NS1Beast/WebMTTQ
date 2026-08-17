namespace WebMTTQ.Models;

public sealed class HomePageViewModel
{
    public List<Banner> Banners { get; set; } = [];
    public List<TrangChuMuc> Sections { get; set; } = new();
    public Dictionary<int, List<TrangChuTinTuc>> SectionNews { get; set; } = new();

    /// <summary>Section "Hành trình chuyển đổi số"</summary>
    public TimelineSection? Timeline { get; set; }

    /// <summary>Danh sách bài viết nổi bật cho section "Tin tức nổi bật"</summary>
    public List<BaiViet> FeaturedNews { get; set; } = new();
}