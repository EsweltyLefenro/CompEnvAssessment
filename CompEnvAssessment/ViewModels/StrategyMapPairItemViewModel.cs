using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public class StrategyMapPairItemViewModel : ViewModelBase
{
    private readonly Action _onPairChanged;

    public StrategyMapPairItemViewModel(
        StrategyMapPair pair,
        string title,
        Action onPairChanged)
    {
        Pair = pair;
        Title = title;
        _onPairChanged = onPairChanged;
    }

    public StrategyMapPair Pair { get; }

    public string Title { get; }

    public string Conclusion
    {
        get => Pair.Conclusion;
        set
        {
            if (Pair.Conclusion == value)
                return;

            Pair.Conclusion = value;
            RefreshStatus();
            _onPairChanged();
        }
    }

    public bool IsSkipped
    {
        get => Pair.IsSkipped;
        set
        {
            if (Pair.IsSkipped == value)
                return;

            Pair.IsSkipped = value;
            RefreshStatus();
            _onPairChanged();
        }
    }

    public bool IsCompleted => Pair.IsCompleted;

    public bool HasTextConclusion => Pair.HasTextConclusion;

    public string StatusMark
    {
        get
        {
            if (HasTextConclusion)
                return "✓";

            if (IsSkipped)
                return "—";

            return "×";
        }
    }

    public string StatusText
    {
        get
        {
            if (HasTextConclusion)
                return "Вывод заполнен";

            if (IsSkipped)
                return "Вывод не требуется";

            return "Не заполнено";
        }
    }

    public void RefreshStatus()
    {
        OnPropertyChanged(nameof(Conclusion));
        OnPropertyChanged(nameof(IsSkipped));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(HasTextConclusion));
        OnPropertyChanged(nameof(StatusMark));
        OnPropertyChanged(nameof(StatusText));
    }
}