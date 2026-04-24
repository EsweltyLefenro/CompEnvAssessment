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
        !string.IsNullOrWhiteSpace(Conclusion);

    public bool IsCompleted =>
        IsSkipped || HasTextConclusion;

    partial void OnConclusionChanged(string value)
    {
        OnPropertyChanged(nameof(HasTextConclusion));
        OnPropertyChanged(nameof(IsCompleted));
    }

    partial void OnIsSkippedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsCompleted));
    }
}