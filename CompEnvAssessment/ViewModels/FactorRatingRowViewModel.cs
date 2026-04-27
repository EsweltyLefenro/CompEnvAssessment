using System.Collections.ObjectModel;
using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public class FactorRatingRowViewModel : ViewModelBase
{
    public FactorRatingRowViewModel(CompetitiveFactor factor)
    {
        Factor = factor;
    }

    public CompetitiveFactor Factor { get; }

    public string FactorName => Factor.Name;

    public decimal Weight => Factor.Weight;

    public ObservableCollection<CompanyScoreCellViewModel> Cells { get; } = new();

    public void Refresh()
    {
        OnPropertyChanged(nameof(FactorName));
        OnPropertyChanged(nameof(Weight));
    }
}