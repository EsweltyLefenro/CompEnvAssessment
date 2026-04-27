using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public class CompanyRatingColumnViewModel : ViewModelBase
{
    private readonly Func<Company, decimal> _calculateRating;
    private readonly Func<Company, bool> _canRemoveCompany;
    private readonly Action _onDisplayChanged;
    private readonly Action _onDataChanged;

    public CompanyRatingColumnViewModel(
        Company company,
        Func<Company, decimal> calculateRating,
        Func<Company, bool> canRemoveCompany,
        Action onDisplayChanged,
        Action onDataChanged)
    {
        Company = company;
        _calculateRating = calculateRating;
        _canRemoveCompany = canRemoveCompany;
        _onDisplayChanged = onDisplayChanged;
        _onDataChanged = onDataChanged;
    }

    public Company Company { get; }

    public bool IsUserCompany => Company.IsUserCompany;

    public bool CanRemove => _canRemoveCompany(Company);

    public string Name
    {
        get => Company.Name;
        set
        {
            if (Company.Name == value)
                return;

            Company.Name = value;
            OnPropertyChanged();
            _onDisplayChanged();
        }
    }

    public decimal MarketShare
    {
        get => Company.MarketShare;
        set
        {
            if (Company.MarketShare == value)
                return;

            Company.MarketShare = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WeightedRating));
            _onDataChanged();
        }
    }

    public decimal WeightedRating => _calculateRating(Company);

    public void Refresh()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(MarketShare));
        OnPropertyChanged(nameof(WeightedRating));
        OnPropertyChanged(nameof(CanRemove));
    }

    public void RefreshRating()
    {
        OnPropertyChanged(nameof(WeightedRating));
    }
}