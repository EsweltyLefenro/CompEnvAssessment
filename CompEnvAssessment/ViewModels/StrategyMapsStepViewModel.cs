using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class StrategyMapsStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly Action _refreshWorkflow;

    public StrategyMapsStepViewModel(
        AppProject project,
        ValidationService validationService,
        StrategyMapService strategyMapService,
        Action refreshWorkflow)
        : base(
            project,
            3,
            "Карты стратегической группировки",
            "Как работать с картами",
            "Выберите пару факторов, изучите карту и заполните вывод. Если по паре факторов содержательный вывод невозможен, отметьте соответствующий флажок. Минимум 3 карты должны иметь текстовый вывод.")
    {
        _validationService = validationService;
        _strategyMapService = strategyMapService;
        _refreshWorkflow = refreshWorkflow;

        StrategyMapPairs.CollectionChanged += StrategyMapPairs_CollectionChanged;
    }

    public ObservableCollection<StrategyMapPair> StrategyMapPairs => Project.StrategyMapPairs;

    public ObservableCollection<StrategyMapPairItemViewModel> PairItems { get; } = new();

    public int TotalPairsCount => PairItems.Count;

    public int CompletedPairsCount => PairItems.Count(pair => pair.IsCompleted);

    public int TextConclusionsCount => PairItems.Count(pair => pair.HasTextConclusion);

    public int RequiredTextConclusionsCount => AppConstants.MinTextStrategyMapConclusions;

    public string TextConclusionsProgressText =>
    $"{TextConclusionsCount} / минимум {RequiredTextConclusionsCount}";

    public bool IsPairSelected => SelectedPairItem is not null;

    public bool IsPairListVisible => SelectedPairItem is null;

    public bool IsPairDetailsVisible => SelectedPairItem is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPairSelected))]
    [NotifyPropertyChangedFor(nameof(IsPairListVisible))]
    [NotifyPropertyChangedFor(nameof(IsPairDetailsVisible))]
    private StrategyMapPairItemViewModel? selectedPairItem;

    public IReadOnlyList<StrategyMapPointViewModel> SelectedPairPoints
    {
        get
        {
            if (SelectedPairItem is null)
                return Array.Empty<StrategyMapPointViewModel>();

            IReadOnlyList<StrategyMapPoint> points = _strategyMapService.BuildPoints(
                Project,
                SelectedPairItem.Pair);

            decimal maxMarketShare = points.Count == 0
                ? 0
                : points.Max(point => point.MarketShare);

            return points
                .Select(point => new StrategyMapPointViewModel(point, maxMarketShare))
                .ToList();
        }
    }

    public string SelectedPairTitle =>
        SelectedPairItem?.Title ?? string.Empty;

    public string SelectedXAxisTitle
    {
        get
        {
            if (SelectedPairItem is null)
                return string.Empty;

            return _strategyMapService.GetXAxisTitle(Project, SelectedPairItem.Pair);
        }
    }

    public string SelectedYAxisTitle
    {
        get
        {
            if (SelectedPairItem is null)
                return string.Empty;

            return _strategyMapService.GetYAxisTitle(Project, SelectedPairItem.Pair);
        }
    }

    [RelayCommand]
    private void OpenPair(StrategyMapPairItemViewModel? pairItem)
    {
        if (pairItem is null)
            return;

        SelectedPairItem = pairItem;

        RefreshSelectedPairProperties();
    }

    [RelayCommand]
    private void BackToPairsList()
    {
        SelectedPairItem = null;

        RefreshSelectedPairProperties();
    }

    public override void OnEnter()
    {
        IReadOnlyList<string> ratingErrors = _validationService.ValidateCompanyRatings(Project);

        if (ratingErrors.Count == 0)
        {
            _strategyMapService.SyncPairs(Project);
        }

        RebuildPairItems();
        SubscribeToAllPairs();
        RefreshCounters();
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateStrategyMaps(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }

    private void StrategyMapPairs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (StrategyMapPair pair in e.NewItems)
            {
                SubscribeToPair(pair);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (StrategyMapPair pair in e.OldItems)
            {
                pair.PropertyChanged -= Pair_PropertyChanged;
            }
        }

        RebuildPairItems();
        RefreshCounters();
    }

    private void RebuildPairItems()
    {
        PairItems.Clear();

        foreach (StrategyMapPair pair in StrategyMapPairs)
        {
            string title = _strategyMapService.GetPairTitle(Project, pair);

            PairItems.Add(new StrategyMapPairItemViewModel(
                pair,
                title,
                RefreshAfterPairChanged));
        }

        if (SelectedPairItem is not null)
        {
            SelectedPairItem = PairItems.FirstOrDefault(item => item.Pair == SelectedPairItem.Pair);
        }

        OnPropertyChanged(nameof(PairItems));
        RefreshSelectedPairProperties();
    }

    private void SubscribeToAllPairs()
    {
        foreach (StrategyMapPair pair in StrategyMapPairs)
        {
            SubscribeToPair(pair);
        }
    }

    private void SubscribeToPair(StrategyMapPair pair)
    {
        pair.PropertyChanged -= Pair_PropertyChanged;
        pair.PropertyChanged += Pair_PropertyChanged;
    }

    private void Pair_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StrategyMapPair.Conclusion)
            or nameof(StrategyMapPair.IsSkipped))
        {
            foreach (StrategyMapPairItemViewModel item in PairItems)
            {
                item.RefreshStatus();
            }

            RefreshCounters();
        }
    }

    private void RefreshAfterPairChanged()
    {
        RefreshCounters();
        RefreshSelectedPairProperties();
        _refreshWorkflow();
    }

    private void RefreshCounters()
    {
        OnPropertyChanged(nameof(TotalPairsCount));
        OnPropertyChanged(nameof(CompletedPairsCount));
        OnPropertyChanged(nameof(TextConclusionsCount));
        OnPropertyChanged(nameof(RequiredTextConclusionsCount));
        OnPropertyChanged(nameof(TextConclusionsProgressText));

        _refreshWorkflow();
    }

    private void RefreshSelectedPairProperties()
    {
        OnPropertyChanged(nameof(SelectedPairPoints));
        OnPropertyChanged(nameof(SelectedPairTitle));
        OnPropertyChanged(nameof(SelectedXAxisTitle));
        OnPropertyChanged(nameof(SelectedYAxisTitle));
        OnPropertyChanged(nameof(IsPairSelected));
        OnPropertyChanged(nameof(IsPairListVisible));
        OnPropertyChanged(nameof(IsPairDetailsVisible));
    }
}
