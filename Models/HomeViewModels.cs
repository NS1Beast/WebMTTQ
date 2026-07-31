namespace WebMTTQ.Models;

public sealed class HomePageViewModel
{
    public List<Banner> Banners { get; set; } = [];
    public List<TrangChuMuc> Sections { get; set; } = new();
    public Dictionary<int, List<TrangChuTinTuc>> SectionNews { get; set; } = new();
}