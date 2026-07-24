namespace WebMTTQ.Models;

public sealed class HomePageViewModel
{
    public FeaturedNewsItem LeadNews { get; init; } = new();
    public IReadOnlyList<NewsItem> SideNews { get; init; } = [];
    public IReadOnlyList<NewsCategoryBlock> Categories { get; init; } = [];
    public IReadOnlyList<TimelineItem> Timeline { get; init; } = [];
    public IReadOnlyList<StatItem> Stats { get; init; } = [];
    public List<Banner> Banners { get; set; } = [];
}

public sealed class FeaturedNewsItem
{
    public string Title { get; init; } = "";
    public string Url { get; init; } = "#";
    public string ImageUrl { get; init; } = "";
    public string Category { get; init; } = "";
    public string Date { get; init; } = "";
}

public sealed class NewsItem
{
    public string Title { get; init; } = "";
    public string Url { get; init; } = "#";
    public string ImageUrl { get; init; } = "";
    public string Category { get; init; } = "";
    public string Date { get; init; } = "";
}

public sealed class NewsCategoryBlock
{
    public string Title { get; init; } = "";
    public string MoreUrl { get; init; } = "#";
    public string StyleClass { get; init; } = "";
    public FeaturedNewsItem? Featured { get; init; }
    public IReadOnlyList<NewsItem> ListItems { get; init; } = [];
    public IReadOnlyList<NewsItem> GridItems { get; init; } = [];
}

public sealed class TimelineItem
{
    public string Date { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public bool IsNext { get; init; }
}

public sealed class StatItem
{
    public int Count { get; init; }
    public string Suffix { get; init; } = "";
    public string Label { get; init; } = "";
}
