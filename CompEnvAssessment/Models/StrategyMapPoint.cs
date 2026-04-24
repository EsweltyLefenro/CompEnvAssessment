namespace CompEnvAssessment.Models;

public class StrategyMapPoint
{
    public required string CompanyName { get; init; }

    public required decimal X { get; init; }

    public required decimal Y { get; init; }

    public required decimal MarketShare { get; init; }

    public required bool IsUserCompany { get; init; }
}