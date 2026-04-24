using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

        Companies.CollectionChanged += Companies_CollectionChanged;

        foreach (Company company in Companies)
        {
            SubscribeToCompany(company);
        }
    }

    public ObservableCollection<CompetitiveFactor> Factors => Project.Factors;

    public ObservableCollection<Company> Companies => Project.Companies;

    public decimal TotalMarketShare => Companies.Sum(company => company.MarketShare);

    [RelayCommand]
    private void AddCompetitor()
    {
        Company company = Project.AddCompetitor();

        SubscribeToCompany(company);

        RefreshAfterRatingsChange();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveCompetitor))]
    private void RemoveCompetitor(Company? company)
    {
        if (company is null)
            return;

        UnsubscribeFromCompany(company);

        bool removed = Project.RemoveCompetitor(company);

        if (removed)
        {
            RefreshAfterRatingsChange();
        }
    }

    private bool CanRemoveCompetitor(Company? company)
    {
        if (company is null)
            return false;

        if (company.IsUserCompany)
            return false;

        int competitorsCount = Companies.Count(item => !item.IsUserCompany);

        return competitorsCount > AppConstants.MinCompetitorsCount;
    }

    public decimal GetWeightedRating(Company company)
    {
        return _ratingCalculationService.CalculateWeightedRating(company, Factors);
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateCompanyRatings(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }

    private void Companies_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (Company company in e.NewItems)
            {
                SubscribeToCompany(company);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (Company company in e.OldItems)
            {
                UnsubscribeFromCompany(company);
            }
        }

        RefreshAfterRatingsChange();
    }

    private void SubscribeToCompany(Company company)
    {
        company.PropertyChanged -= Company_PropertyChanged;
        company.PropertyChanged += Company_PropertyChanged;

        company.Scores.CollectionChanged -= Scores_CollectionChanged;
        company.Scores.CollectionChanged += Scores_CollectionChanged;

        foreach (CompanyScore score in company.Scores)
        {
            SubscribeToScore(score);
        }
    }

    private void UnsubscribeFromCompany(Company company)
    {
        company.PropertyChanged -= Company_PropertyChanged;
        company.Scores.CollectionChanged -= Scores_CollectionChanged;

        foreach (CompanyScore score in company.Scores)
        {
            score.PropertyChanged -= Score_PropertyChanged;
        }
    }

    private void Scores_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (CompanyScore score in e.NewItems)
            {
                SubscribeToScore(score);
            }
        }

        if (e.OldItems is not null)
        {
            foreach (CompanyScore score in e.OldItems)
            {
                score.PropertyChanged -= Score_PropertyChanged;
            }
        }

        RefreshAfterRatingsChange();
    }

    private void SubscribeToScore(CompanyScore score)
    {
        score.PropertyChanged -= Score_PropertyChanged;
        score.PropertyChanged += Score_PropertyChanged;
    }

    private void Company_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Company.MarketShare))
        {
            RefreshAfterRatingsChange();
            return;
        }

        if (e.PropertyName == nameof(Company.Name))
        {
            _refreshWorkflow();
        }
    }

    private void Score_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CompanyScore.Score))
        {
            RefreshAfterRatingsChange();
        }
    }

    private void RefreshAfterRatingsChange()
    {
        _strategyMapService.ResetMapsAndSummary(Project);

        OnPropertyChanged(nameof(TotalMarketShare));

        foreach (Company company in Companies)
        {
            OnPropertyChanged(nameof(company));
        }

        RemoveCompetitorCommand.NotifyCanExecuteChanged();

        _refreshWorkflow();
    }
}