using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public partial class StrategyMapPair : ObservableObject
{
    public StrategyMapPair(Guid xFactorId, Guid yFactorId)
    {
        XFactorId = xFactorId;
        YFactorId = yFactorId;
    }

    public Guid XFactorId { get; }

    public Guid YFactorId { get; }

    [ObservableProperty]
    private string conclusion = string.Empty;

    [ObservableProperty]
    private bool isSkipped;

    public bool HasTextConclusion =>
        !IsSkipped && !string.IsNullOrWhiteSpace(Conclusion);

    public bool IsCompleted =>
        IsSkipped || HasTextConclusion;

    partial void OnConclusionChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && IsSkipped)
        {
            IsSkipped = false;
        }

        OnPropertyChanged(nameof(HasTextConclusion));
        OnPropertyChanged(nameof(IsCompleted));
    }

    partial void OnIsSkippedChanged(bool value)
    {
        if (value)
        {
            Conclusion = string.Empty;
        }

        OnPropertyChanged(nameof(HasTextConclusion));
        OnPropertyChanged(nameof(IsCompleted));
    }
}