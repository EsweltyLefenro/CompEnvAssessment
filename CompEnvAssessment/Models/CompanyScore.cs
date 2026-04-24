using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public class CompanyScore : ObservableObject
{
    private int? _score;

    public CompanyScore(Guid factorId)
    {
        FactorId = factorId;
    }

    public Guid FactorId { get; }

    public int? Score
    {
        get => _score;
        set
        {
            int? normalizedValue = value;

            if (normalizedValue is not null)
            {
                normalizedValue = Math.Clamp(
                    normalizedValue.Value,
                    AppConstants.MinScore,
                    AppConstants.MaxScore);
            }

            SetProperty(ref _score, normalizedValue);
        }
    }

    public bool IsFilled => Score is not null;
}