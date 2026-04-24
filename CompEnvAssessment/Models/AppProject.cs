using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public partial class AppProject : ObservableObject
{
    [ObservableProperty]
    private UserProfile userProfile = new();

    [ObservableProperty]
    private EnterpriseProfile enterpriseProfile = new();

    [ObservableProperty]
    private string summary = string.Empty;

    public ObservableCollection<CompetitiveFactor> Factors { get; } = new();

    public ObservableCollection<Company> Companies { get; } = new();

    public ObservableCollection<StrategyMapPair> StrategyMapPairs { get; } = new();

    public decimal TotalWeight =>
        Factors.Sum(factor => factor.Weight);

    public int TextStrategyMapConclusionsCount =>
        StrategyMapPairs.Count(pair => pair.HasTextConclusion);

    public bool HasValidFactors =>
        Factors.Count >= AppConstants.MinFactorsCount
        && Factors.Count <= AppConstants.MaxFactorsCount
        && Factors.All(factor => factor.IsFilled)
        && TotalWeight == AppConstants.RequiredWeightSum;

    public bool HasEnoughTextStrategyMapConclusions =>
        TextStrategyMapConclusionsCount >= AppConstants.MinTextStrategyMapConclusions;

    public bool HasCompletedAllStrategyMapPairs =>
        StrategyMapPairs.Count > 0
        && StrategyMapPairs.All(pair => pair.IsCompleted)
        && HasEnoughTextStrategyMapConclusions;

    public void InitializeDefaultCompanies()
    {
        Companies.Clear();

        Companies.Add(new Company
        {
            Name = "Вы",
            IsUserCompany = true
        });

        for (int i = 0; i < AppConstants.MinCompetitorsCount; i++)
        {
            AddCompetitor();
        }

        SyncCompanyScoresWithFactors();
    }

    public Company AddCompetitor()
    {
        string name = GetNextCompetitorName();

        var company = new Company
        {
            Name = name,
            IsUserCompany = false
        };

        foreach (CompetitiveFactor factor in Factors)
        {
            company.Scores.Add(new CompanyScore(factor.Id));
        }

        Companies.Add(company);
        return company;
    }

    public bool RemoveCompetitor(Company company)
    {
        if (company.IsUserCompany)
            return false;

        int competitorsCount = Companies.Count(c => !c.IsUserCompany);

        if (competitorsCount <= AppConstants.MinCompetitorsCount)
            return false;

        return Companies.Remove(company);
    }

    public void SyncCompanyScoresWithFactors()
    {
        HashSet<Guid> factorIds = Factors
            .Select(factor => factor.Id)
            .ToHashSet();

        foreach (Company company in Companies)
        {
            List<CompanyScore> scoresToRemove = company.Scores
                .Where(score => !factorIds.Contains(score.FactorId))
                .ToList();

            foreach (CompanyScore score in scoresToRemove)
            {
                company.Scores.Remove(score);
            }

            foreach (CompetitiveFactor factor in Factors)
            {
                bool scoreExists = company.Scores
                    .Any(score => score.FactorId == factor.Id);

                if (!scoreExists)
                {
                    company.Scores.Add(new CompanyScore(factor.Id));
                }
            }
        }
    }

    public void ResetStrategyMapPairs()
    {
        StrategyMapPairs.Clear();
    }

    public void RebuildStrategyMapPairs()
    {
        StrategyMapPairs.Clear();

        List<CompetitiveFactor> factors = Factors.ToList();

        for (int i = 0; i < factors.Count; i++)
        {
            for (int j = i + 1; j < factors.Count; j++)
            {
                StrategyMapPairs.Add(new StrategyMapPair(
                    factors[i].Id,
                    factors[j].Id));
            }
        }
    }

    private string GetNextCompetitorName()
    {
        HashSet<string> existingNames = Companies
            .Where(company => !company.IsUserCompany)
            .Select(company => company.Name)
            .ToHashSet();

        int number = 1;

        while (existingNames.Contains($"Компания {number}"))
        {
            number++;
        }

        return $"Компания {number}";
    }
}
