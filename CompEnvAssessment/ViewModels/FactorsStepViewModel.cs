using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class FactorsStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly Action _refreshWorkflow;

    public FactorsStepViewModel(
        AppProject project,
        ValidationService validationService,
        StrategyMapService strategyMapService,
        Action refreshWorkflow)
        : base(
            project,
            1,
            "Факторы и веса",
            "Как заполнить факторы и веса",
            "Добавьте от 3 до 20 факторов конкурентоспособности. Для каждого фактора укажите вес в формате 0,00. Сумма весов должна быть равна 1,00.")
    {
        _validationService = validationService;
        _strategyMapService = strategyMapService;
        _refreshWorkflow = refreshWorkflow;

        Factors.CollectionChanged += Factors_CollectionChanged;

        foreach (CompetitiveFactor factor in Factors)
        {
            SubscribeToFactor(factor);
        }
    }

    public ObservableCollection<CompetitiveFactor> Factors => Project.Factors;

    public decimal TotalWeight => Project.TotalWeight;

    public bool CanAddFactor => Factors.Count < AppConstants.MaxFactorsCount;

    [RelayCommand(CanExecute = nameof(CanAddFactor))]
    private void AddFactor()
    {
        var factor = new CompetitiveFactor
        {
            Name = $"Фактор {Factors.Count + 1}",
            Weight = 0
        };

        Factors.Add(factor);
    }

    [RelayCommand]
    private void RemoveFactor(CompetitiveFactor? factor)
    {
        if (factor is null)
            return;

        factor.PropertyChanged -= Factor_PropertyChanged;

        Factors.Remove(factor);
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateFactors(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }

    private void Factors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (CompetitiveFactor factor in e.NewItems)
            {
                SubscribeToFactor(factor);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (CompetitiveFactor factor in e.OldItems)
            {
                factor.PropertyChanged -= Factor_PropertyChanged;
            }
        }

        RefreshAfterChange();
    }

    private void SubscribeToFactor(CompetitiveFactor factor)
    {
        factor.PropertyChanged -= Factor_PropertyChanged;
        factor.PropertyChanged += Factor_PropertyChanged;
    }

    private void Factor_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CompetitiveFactor.Name) or nameof(CompetitiveFactor.Weight))
        {
            RefreshAfterChange();
        }
    }

    private void RefreshAfterChange()
    {
        Project.SyncCompanyScoresWithFactors();
        _strategyMapService.SyncPairs(Project);

        OnPropertyChanged(nameof(TotalWeight));
        OnPropertyChanged(nameof(CanAddFactor));

        AddFactorCommand.NotifyCanExecuteChanged();

        _refreshWorkflow();
    }
}
