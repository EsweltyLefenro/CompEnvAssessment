using CompEnvAssessment.Models;

namespace CompEnvAssessment.Services;

public class RatingCalculationService
{
    public decimal CalculateWeightedRating(Company company, IEnumerable<CompetitiveFactor> factors)
    {
        decimal result = 0;

        foreach (CompetitiveFactor factor in factors)
        {
            CompanyScore? score = company.Scores
                .FirstOrDefault(item => item.FactorId == factor.Id);

            if (score?.Score is null)
                continue;

            result += score.Score.Value * factor.Weight;
        }

        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }
}