namespace WebMTTQ.Models;

public sealed class AboutPageViewModel
{
    public string PageTitle { get; init; } = "Giới thiệu";
    public string PageSubtitle { get; init; } = "";
    public string Eyebrow { get; init; } = "";
    public string HeroEyebrow { get; init; } = "Ủy ban Mặt trận Tổ quốc Việt Nam · PHƯỜNG TÂN ĐỊNH";
    public string HeroTitle { get; init; } = "Giới thiệu";
    public string HeroSubtitle { get; init; } = "Ban Thường trực Ủy ban Mặt trận Tổ quốc Việt Nam PHƯỜNG TÂN ĐỊNH — tập thể đoàn kết, gần dân, hết lòng vì nhân dân phục vụ.";

    // Dấu mốc thành lập
    public string FoundEyebrow { get; init; } = "Dấu mốc thành lập";
    public string FoundTitle { get; init; } = "Kiện toàn tổ chức, vững bước phụng sự nhân dân";
    public string FoundContent { get; init; } = "";
    public string FoundDecisionNumber { get; init; } = "291/QĐ-MTTQ-BTT";
    public string FoundDecisionDate { get; init; } = "Ban hành 27/6/2025";
    public string FoundAnnounceDate { get; init; } = "Công bố 01/7/2025";

    // Thống kê
    public IReadOnlyList<StatItem> Stats { get; init; } = [];

    // Ban Thường trực
    public string TeamEyebrow { get; init; } = "Nhân sự chủ chốt";
    public string TeamTitle { get; init; } = "Ban Thường trực Ủy ban MTTQ Việt Nam xã";
    public TeamMember? Chairman { get; init; }
    public IReadOnlyList<TeamMember> Members { get; init; } = [];

    // Gallery
    public string GalleryEyebrow { get; init; } = "Khoảnh khắc";
    public string GalleryTitle { get; init; } = "Lễ công bố & ra mắt Ban Thường trực";
    public string GalleryImageUrl { get; init; } = "";
    public string GalleryImageAlt { get; init; } = "";

    // CTA
    public string CtaTitle { get; init; } = "Đồng hành cùng Mặt trận";
    public string CtaText { get; init; } = "Theo dõi văn bản công khai hoặc gửi phản ánh, góp ý trực tiếp đến Ủy ban MTTQ Việt Nam PHƯỜNG TÂN ĐỊNH.";
    public string CtaDocsUrl { get; init; } = "#";
    public string CtaDocsLabel { get; init; } = "Văn bản & Tài liệu";
    public string CtaFeedbackUrl { get; init; } = "#";
    public string CtaFeedbackLabel { get; init; } = "Gửi góp ý";
    public string ShareUrl { get; init; } = "";
    public string ShareTitle { get; init; } = "";
}

public sealed class StatItem
{
    public int Count { get; init; }
    public string Suffix { get; init; } = "";
    public string Label { get; init; } = "";
}

public sealed class TeamMember
{
    public string Name { get; init; } = "";
    public string Role { get; init; } = "";
    public string Pill { get; init; } = "";
    public string PhotoUrl { get; init; } = "";
    public string PhotoFullUrl { get; init; } = "";
    public string PhotoPosition { get; init; } = "center 22%";
    public bool IsChairman { get; init; }
}