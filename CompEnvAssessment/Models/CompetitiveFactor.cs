using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public class CompetitiveFactor : ObservableObject
{
    private string _name = string.Empty;
    private decimal _weight;

    public Guid Id { get; } = Guid.NewGuid();

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, NormalizeWeight(value));
    }

    public bool IsFilled =>
        !string.IsNullOrWhiteSpace(Name)
        && Weight > 0
        && Weight < 1;

    private static decimal NormalizeWeight(decimal value)
    {
        if (value < AppConstants.MinWeight)
            return AppConstants.MinWeight;

        if (value > AppConstants.MaxWeight)
            return AppConstants.MaxWeight;

        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
