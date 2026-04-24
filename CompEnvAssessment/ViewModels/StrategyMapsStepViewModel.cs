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

    public int TotalPairsCount => StrategyMapPairs.Count;

    public int CompletedPairsCount => StrategyMapPairs.Count(pair => pair.IsCompleted);

    public int TextConclusionsCount => StrategyMapPairs.Count(pair => pair.HasTextConclusion);

    public bool IsPairSelected => SelectedPair is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPairSelected))]
    private StrategyMapPair? selectedPair;

    public IReadOnlyList<StrategyMapPoint> SelectedPairPoints
    {
        get
        {
            if (SelectedPair is null)
                return Array.Empty<StrategyMapPoint>();

            return _strategyMapService.BuildPoints(Project, SelectedPair);
        }
    }

    public string SelectedPairTitle
    {
        get
        {
            if (SelectedPair is null)
                return string.Empty;

            return _strategyMapService.GetPairTitle(Project, SelectedPair);
        }
    }

    public string SelectedXAxisTitle
    {
        get
        {
            if (SelectedPair is null)
                return string.Empty;

            return _strategyMapService.GetXAxisTitle(Project, SelectedPair);
        }
    }

    public string SelectedYAxisTitle
    {
        get
        {
            if (SelectedPair is null)
                return string.Empty;

            return _strategyMapService.GetYAxisTitle(Project, SelectedPair);
        }
    }

    [RelayCommand]
    private void OpenPair(StrategyMapPair? pair)
    {
        if (pair is null)
            return;

        SelectedPair = pair;

        OnPropertyChanged(nameof(SelectedPairPoints));
        OnPropertyChanged(nameof(SelectedPairTitle));
        OnPropertyChanged(nameof(SelectedXAxisTitle));
        OnPropertyChanged(nameof(SelectedYAxisTitle));
    }

    [RelayCommand]
    private void BackToPairsList()
    {
        SelectedPair = null;
    }

    public override void OnEnter()
    {
        if (Project.StrategyMapPairs.Count == 0)
        {
            IReadOnlyList<string> ratingErrors = _validationService.ValidateCompanyRatings(Project);

            if (ratingErrors.Count == 0)
            {
                _strategyMapService.RebuildPairs(Project);
            }
        }

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

    public string GetPairTitle(StrategyMapPair pair)
    {
        return _strategyMapService.GetPairTitle(Project, pair);
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

        RefreshCounters();
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
            RefreshCounters();
        }
    }

    private void RefreshCounters()
    {
        OnPropertyChanged(nameof(TotalPairsCount));
        OnPropertyChanged(nameof(CompletedPairsCount));
        OnPropertyChanged(nameof(TextConclusionsCount));
        OnPropertyChanged(nameof(SelectedPairPoints));

        _refreshWorkflow();
    }
}