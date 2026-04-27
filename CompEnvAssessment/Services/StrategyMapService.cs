using CompEnvAssessment.Models;

namespace CompEnvAssessment.Services;

public class StrategyMapService
{
    public void RebuildPairs(AppProject project)
    {
        project.StrategyMapPairs.Clear();

        SyncPairs(project);
    }

    public void SyncPairs(AppProject project)
    {
        List<CompetitiveFactor> factors = project.Factors.ToList();
        HashSet<Guid> factorIds = factors
            .Select(factor => factor.Id)
            .ToHashSet();

        List<StrategyMapPair> obsoletePairs = project.StrategyMapPairs
            .Where(pair => !factorIds.Contains(pair.XFactorId)
                || !factorIds.Contains(pair.YFactorId))
            .ToList();

        foreach (StrategyMapPair pair in obsoletePairs)
        {
            project.StrategyMapPairs.Remove(pair);
        }

        HashSet<(Guid First, Guid Second)> existingPairKeys = project.StrategyMapPairs
            .Select(pair => GetPairKey(pair.XFactorId, pair.YFactorId))
            .ToHashSet();

        for (int i = 0; i < factors.Count; i++)
        {
            for (int j = i + 1; j < factors.Count; j++)
            {
                (Guid First, Guid Second) pairKey = GetPairKey(
                    factors[i].Id,
                    factors[j].Id);

                if (existingPairKeys.Contains(pairKey))
                    continue;

                project.StrategyMapPairs.Add(new StrategyMapPair(
                    factors[i].Id,
                    factors[j].Id));

                existingPairKeys.Add(pairKey);
            }
        }
    }

    public IReadOnlyList<StrategyMapPoint> BuildPoints(AppProject project, StrategyMapPair pair)
    {
        List<StrategyMapPoint> points = new();

        foreach (Company company in project.Companies)
        {
            CompanyScore? xScore = company.Scores.FirstOrDefault(score => score.FactorId == pair.XFactorId);
            CompanyScore? yScore = company.Scores.FirstOrDefault(score => score.FactorId == pair.YFactorId);

            if (xScore?.Score is null || yScore?.Score is null)
                continue;

            points.Add(new StrategyMapPoint
            {
                CompanyName = company.Name,
                X = xScore.Score.Value,
                Y = yScore.Score.Value,
                MarketShare = company.MarketShare,
                IsUserCompany = company.IsUserCompany
            });
        }

        return points;
    }

    public string GetPairTitle(AppProject project, StrategyMapPair pair)
    {
        string xName = GetFactorName(project, pair.XFactorId);
        string yName = GetFactorName(project, pair.YFactorId);

        return $"{xName} × {yName}";
    }

    public string GetXAxisTitle(AppProject project, StrategyMapPair pair)
    {
        return GetFactorName(project, pair.XFactorId);
    }

    public string GetYAxisTitle(AppProject project, StrategyMapPair pair)
    {
        return GetFactorName(project, pair.YFactorId);
    }

    private static string GetFactorName(AppProject project, Guid factorId)
    {
        return project.Factors.FirstOrDefault(factor => factor.Id == factorId)?.Name
            ?? "Неизвестный фактор";
    }

    private static (Guid First, Guid Second) GetPairKey(Guid firstFactorId, Guid secondFactorId)
    {
        return firstFactorId.CompareTo(secondFactorId) <= 0
            ? (firstFactorId, secondFactorId)
            : (secondFactorId, firstFactorId);
    }
}
