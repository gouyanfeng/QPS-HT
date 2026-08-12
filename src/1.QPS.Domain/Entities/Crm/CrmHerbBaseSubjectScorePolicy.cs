namespace QPS.Domain.Entities.Crm;

public class CrmHerbBaseSubjectScoreInput
{
    public string Status { get; init; } = string.Empty;

    public decimal Scale { get; init; }

    public int BaseCount { get; init; }

    public int MainProductCount { get; init; }

    public bool HasPrimaryContactName { get; init; }

    public bool HasPrimaryContactPhone { get; init; }

    public bool HasValidContact { get; init; }

    public bool HasValidContactPhone { get; init; }

    public string LastFollowResult { get; init; } = string.Empty;

    public DateTime? LastFollowAt { get; init; }

    public DateTime Now { get; init; } = DateTime.Now;

    public bool HasRegion { get; init; }

    public bool HasAddress { get; init; }

    public IReadOnlyCollection<string> SourcePlatforms { get; init; } = [];

    public bool HasRemark { get; init; }
}

public readonly record struct CrmHerbBaseSubjectScoreResult(int Score, string Grade);

public static class CrmHerbBaseSubjectScorePolicy
{
    private const string GradeA = "高";
    private const string GradeB = "中";
    private const string GradeC = "低";
    private const string InvalidGrade = "无效";
    private static readonly string[] LostStatuses = ["LOST", "已流失"];
    private static readonly string[] InterestedStatuses = ["INTERESTED", "有意向"];
    private static readonly string[] DealStatuses = ["DEAL", "已成交"];
    private static readonly string[] FollowingStatuses = ["FOLLOWING", "跟进中"];
    private static readonly string[] EffectiveFollowResults = ["CONNECTED", "INTERESTED", "已接通", "有意向"];

    public static string NormalizeGrade(string? grade)
    {
        return grade?.Trim() switch
        {
            "A" => GradeA,
            "B" => GradeB,
            "C" => GradeC,
            "INVALID" => InvalidGrade,
            "" or null => string.Empty,
            var value => value
        };
    }

    public static CrmHerbBaseSubjectScoreResult Calculate(CrmHerbBaseSubjectScoreInput input)
    {
        if (LostStatuses.Contains(input.Status))
        {
            return new CrmHerbBaseSubjectScoreResult(0, InvalidGrade);
        }

        var score =
            ScaleScore(input.Scale) +
            BaseCountScore(input.BaseCount) +
            MainProductScore(input.MainProductCount) +
            ContactScore(input) +
            FollowScore(input) +
            SourceScore(input.SourcePlatforms) +
            DataScore(input);

        if (InterestedStatuses.Contains(input.Status))
        {
            score = Math.Max(score, 60);
        }

        if (input.BaseCount == 0)
        {
            score = CapScore(score, 59);
        }

        if (!input.HasPrimaryContactPhone)
        {
            score = CapScore(score, 79);
        }

        score = Math.Clamp(score, 0, 100);
        return new CrmHerbBaseSubjectScoreResult(score, ToGrade(score));
    }

    private static int ScaleScore(decimal scale)
    {
        if (scale >= 500) return 25;
        if (scale >= 200) return 20;
        if (scale >= 100) return 15;
        if (scale > 0) return 10;
        return 0;
    }

    private static int BaseCountScore(int baseCount)
    {
        if (baseCount >= 3) return 10;
        if (baseCount == 2) return 8;
        if (baseCount == 1) return 5;
        return 0;
    }

    private static int MainProductScore(int mainProductCount)
    {
        if (mainProductCount >= 2) return 15;
        if (mainProductCount == 1) return 10;
        return 0;
    }

    private static int ContactScore(CrmHerbBaseSubjectScoreInput input)
    {
        var score = 0;
        if (input.HasPrimaryContactPhone) score += 12;
        if (input.HasPrimaryContactName) score += 4;
        if (input.HasValidContactPhone) score += 4;
        else if (input.HasValidContact) score += 2;
        return Math.Min(score, 20);
    }

    private static int FollowScore(CrmHerbBaseSubjectScoreInput input)
    {
        if (DealStatuses.Contains(input.Status)) return 20;
        if (InterestedStatuses.Contains(input.Status)) return 18;
        if (IsRecentEffectiveFollow(input)) return 14;
        if (FollowingStatuses.Contains(input.Status)) return 10;
        return 0;
    }

    private static bool IsRecentEffectiveFollow(CrmHerbBaseSubjectScoreInput input)
    {
        return input.LastFollowAt.HasValue &&
            input.LastFollowAt.Value >= input.Now.AddDays(-30) &&
            EffectiveFollowResults.Contains(input.LastFollowResult);
    }

    private static int DataScore(CrmHerbBaseSubjectScoreInput input)
    {
        var score = 0;
        if (input.HasRegion) score += 2;
        if (input.HasAddress) score += 2;
        if (input.HasRemark) score += 1;
        return score;
    }

    private static int SourceScore(IReadOnlyCollection<string> sourcePlatforms)
    {
        return sourcePlatforms
            .Select(SourcePlatformScore)
            .DefaultIfEmpty(0)
            .Max();
    }

    private static int SourcePlatformScore(string? sourcePlatform)
    {
        if (string.IsNullOrWhiteSpace(sourcePlatform)) return 0;

        var normalized = sourcePlatform.Trim().ToUpperInvariant();

        return normalized switch
        {
            "MANUAL" or "人工录入" or "手工录入" => 5,
            "GOV" or "GAP" or "GOV_HERB_BASE" or "政府网站" or "官方公示" => 5,
            "HERB_PLATFORM" or "INDUSTRY_SITE" or "THIRD_PARTY" or "YT1998" or "ZYCTD" => 4,
            "BAIDU_MAP" or "MAP_POI" or "百度地图" => 3,
            _ => 2
        };
    }

    private static int CapScore(int score, int maxScore) => Math.Min(score, maxScore);

    private static string ToGrade(int score)
    {
        if (score >= 80) return GradeA;
        if (score >= 60) return GradeB;
        if (score >= 30) return GradeC;
        return InvalidGrade;
    }
}
