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

    // Trạng thái chuyên mục hiện tại
    public string CurrentCategoryName { get; init; } = "Tin tức";
    public bool HasArticles { get; init; } = true;
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

/// <summary>
/// ViewModel cho trang chi tiết bài viết.
/// </summary>
public sealed class NewsDetailViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string? Excerpt { get; init; }
    public string? Content { get; init; }
    public string? ImageUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string Date { get; init; } = "";
    public string Author { get; init; } = "";
    public int ViewCount { get; init; }
    public string CategoryName { get; init; } = "";
    public string CategorySlug { get; init; } = "";
    public IReadOnlyList<NewsArticleItem> RelatedArticles { get; init; } = [];
}
