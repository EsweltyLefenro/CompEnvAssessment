using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class CompanyRatingStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;
    private readonly RatingCalculationService _ratingCalculationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly Action _refreshWorkflow;

    public CompanyRatingStepViewModel(
        AppProject project,
        ValidationService validationService,
        RatingCalculationService ratingCalculationService,
        StrategyMapService strategyMapService,
        Action refreshWorkflow)
        : base(
            project,
            2,
            "Оценка компаний",
            "Как оценивать компании",
            "Укажите долю рынка и оценки компаний по каждому фактору. Оценки вводятся по шкале от 1 до 10.")
    {
        _validationService = validationService;
        _ratingCalculationService = ratingCalculationService;
        _strategyMapService = strategyMapService;
        _refreshWorkflow = refreshWorkflow;

        Project.Factors.CollectionChanged += Factors_CollectionChanged;
        Project.Companies.CollectionChanged += Companies_CollectionChanged;

        RebuildDisplayModel();
    }

    public ObservableCollection<CompanyRatingColumnViewModel> CompanyColumns { get; } = new();

    public ObservableCollection<FactorRatingRowViewModel> RatingRows { get; } = new();

    public decimal TotalMarketShare => Project.Companies.Sum(company => company.MarketShare);

    public bool HasCompanies => CompanyColumns.Count > 0;

    [RelayCommand]
    private void AddCompetitor()
    {
        Project.AddCompetitor();

        RebuildDisplayModel();
        RefreshAfterDataChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveCompetitor))]
    private void RemoveCompetitor(CompanyRatingColumnViewModel? column)
    {
        if (column is null)
            return;

        bool removed = Project.RemoveCompetitor(column.Company);

        if (!removed)
            return;

        RebuildDisplayModel();
        RefreshAfterDataChanged();
    }

    private bool CanRemoveCompetitor(CompanyRatingColumnViewModel? column)
    {
        if (column is null)
            return false;

        return CanRemoveCompany(column.Company);
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateCompanyRatings(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }

    private void Factors_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Project.SyncCompanyScoresWithFactors();

        RebuildDisplayModel();
        RefreshAfterDataChanged();
    }

    private void Companies_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildDisplayModel();
        RefreshAfterDataChanged();
    }

    private void RebuildDisplayModel()
    {
        Project.SyncCompanyScoresWithFactors();

        CompanyColumns.Clear();
        RatingRows.Clear();

        foreach (Company company in Project.Companies)
        {
            CompanyColumns.Add(new CompanyRatingColumnViewModel(
                company,
                CalculateWeightedRating,
                CanRemoveCompany,
                RefreshAfterDisplayChanged,
                RefreshAfterDataChanged));
        }

        foreach (CompetitiveFactor factor in Project.Factors)
        {
            var row = new FactorRatingRowViewModel(factor);

            foreach (CompanyRatingColumnViewModel column in CompanyColumns)
            {
                CompanyScore score = GetOrCreateScore(column.Company, factor);

                row.Cells.Add(new CompanyScoreCellViewModel(
                    factor,
                    column,
                    score,
                    RefreshAfterDataChanged));
            }

            RatingRows.Add(row);
        }

        RefreshColumns();
        RefreshRows();

        OnPropertyChanged(nameof(HasCompanies));
        OnPropertyChanged(nameof(TotalMarketShare));

        RemoveCompetitorCommand.NotifyCanExecuteChanged();
    }

    private CompanyScore GetOrCreateScore(Company company, CompetitiveFactor factor)
    {
        CompanyScore? score = company.Scores
            .FirstOrDefault(item => item.FactorId == factor.Id);

        if (score is not null)
            return score;

        score = new CompanyScore(factor.Id);
        company.Scores.Add(score);

        return score;
    }

    private decimal CalculateWeightedRating(Company company)
    {
        return _ratingCalculationService.CalculateWeightedRating(company, Project.Factors);
    }

    private bool CanRemoveCompany(Company company)
    {
        if (company.IsUserCompany)
            return false;

        int competitorsCount = Project.Companies.Count(item => !item.IsUserCompany);

        return competitorsCount > AppConstants.MinCompetitorsCount;
    }

    private void RefreshAfterDisplayChanged()
    {
        RefreshColumns();
        _refreshWorkflow();
    }

    private void RefreshAfterDataChanged()
    {
        _strategyMapService.ResetMapsAndSummary(Project);

        OnPropertyChanged(nameof(TotalMarketShare));

        RefreshColumns();
        RefreshRows();

        _refreshWorkflow();
    }

    private void RefreshColumns()
    {
        foreach (CompanyRatingColumnViewModel column in CompanyColumns)
        {
            column.Refresh();
        }

        RemoveCompetitorCommand.NotifyCanExecuteChanged();
    }

    private void RefreshRows()
    {
        foreach (FactorRatingRowViewModel row in RatingRows)
        {
            row.Refresh();
        }
    }
}