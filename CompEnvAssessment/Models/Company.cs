using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public class Company : ObservableObject
{
    private string _name = string.Empty;
    private decimal _marketShare;

    public Guid Id { get; } = Guid.NewGuid();

    public bool IsUserCompany { get; init; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal MarketShare
    {
        get => _marketShare;
        set => SetProperty(ref _marketShare, NormalizeMarketShare(value));
    }

    public ObservableCollection<CompanyScore> Scores { get; } = new();

    public bool CanBeDeleted => !IsUserCompany;

    public bool HasAllScores =>
        Scores.Count > 0
        && Scores.All(score => score.IsFilled);

    private static decimal NormalizeMarketShare(decimal value)
    {
        if (value < AppConstants.MinMarketShare)
            return AppConstants.MinMarketShare;

        if (value > AppConstants.MaxMarketShare)
            return AppConstants.MaxMarketShare;

        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}