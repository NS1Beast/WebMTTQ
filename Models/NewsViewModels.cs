namespace WebMTTQ.Models;

public sealed class NewsIndexViewModel
{
    public string PageTitle { get; init; } = "Tin tức";
    public string HeroTitle { get; init; } = "Danh mục: Tin tức";
    public string HeroEyebrow { get; init; } = "Chuyên mục";
    public string CurrentCategory { get; init; } = "tin-tuc";
    public IReadOnlyList<NewsCategoryInfo> Categories { get; init; } = [];
    public IReadOnlyList<NewsArticleItem> Articles { get; init; } = [];
    public NewsPaginationInfo Pagination { get; init; } = new();

    // Sidebar
    public IReadOnlyList<SidebarDocItem> RecentDocs { get; init; } = [];
}

public sealed class NewsCategoryInfo
{
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string Url { get; init; } = "#";
    public bool IsActive { get; set; }
}

public sealed class NewsArticleItem
{
    public string Title { get; init; } = "";
    public string Url { get; init; } = "#";
    public string ImageUrl { get; init; } = "";
    public string Date { get; init; } = "";
    public string Excerpt { get; init; } = "";
}

public sealed class NewsPaginationInfo
{
    public int CurrentPage { get; init; } = 1;
    public int TotalPages { get; init; } = 1;
    public string BaseUrl { get; init; } = "#";
}

public sealed class SidebarDocItem
{
    public string Title { get; init; } = "";
    public string Url { get; init; } = "#";
}