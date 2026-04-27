using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class SummaryStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly Action _refreshWorkflow;

    public SummaryStepViewModel(
        AppProject project,
        ValidationService validationService,
        StrategyMapService strategyMapService,
        Action refreshWorkflow)
        : base(
            project,
            4,
            "Итоги",
            "Как сформулировать итог",
            "Изучите промежуточные выводы по картам стратегической группировки и на их основе сформулируйте общий вывод о конкурентоспособности предприятия.")
    {
        _validationService = validationService;
        _strategyMapService = strategyMapService;
        _refreshWorkflow = refreshWorkflow;

        Project.StrategyMapPairs.CollectionChanged += StrategyMapPairs_CollectionChanged;

        RebuildIntermediateConclusions();
    }

    public ObservableCollection<StrategyMapConclusionItemViewModel> IntermediateConclusions { get; } = new();

    public string Summary
    {
        get => Project.Summary;
        set
        {
            if (Project.Summary == value)
                return;

            Project.Summary = value;
            OnPropertyChanged();
            _refreshWorkflow();
        }
    }

    public int IntermediateConclusionsCount =>
        IntermediateConclusions.Count;

    public int TextIntermediateConclusionsCount =>
        IntermediateConclusions.Count(item => item.HasText);

    public string IntermediateConclusionsCounterText =>
        $"Текстовых выводов: {TextIntermediateConclusionsCount} из {IntermediateConclusionsCount}";

    public override void OnEnter()
    {
        RebuildIntermediateConclusions();
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateSummary(Project);

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

        RebuildIntermediateConclusions();
    }

    private void SubscribeToAllPairs()
    {
        foreach (StrategyMapPair pair in Project.StrategyMapPairs)
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
            RebuildIntermediateConclusions();
        }
    }

    private void RebuildIntermediateConclusions()
    {
        SubscribeToAllPairs();

        IntermediateConclusions.Clear();

        foreach (StrategyMapPair pair in Project.StrategyMapPairs)
        {
            string title = _strategyMapService.GetPairTitle(Project, pair);

            IntermediateConclusions.Add(new StrategyMapConclusionItemViewModel(
                title,
                pair.Conclusion,
                pair.IsSkipped));
        }

        OnPropertyChanged(nameof(IntermediateConclusionsCount));
        OnPropertyChanged(nameof(TextIntermediateConclusionsCount));
        OnPropertyChanged(nameof(IntermediateConclusionsCounterText));
    }
}