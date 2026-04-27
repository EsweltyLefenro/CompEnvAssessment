using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public class CompanyScoreCellViewModel : ViewModelBase
{
    private readonly CompanyScore _score;
    private readonly CompanyRatingColumnViewModel _companyColumn;
    private readonly Action _onScoreChanged;

    public CompanyScoreCellViewModel(
        CompetitiveFactor factor,
        CompanyRatingColumnViewModel companyColumn,
        CompanyScore score,
        Action onScoreChanged)
    {
        Factor = factor;
        _companyColumn = companyColumn;
        _score = score;
        _onScoreChanged = onScoreChanged;
    }

    public CompetitiveFactor Factor { get; }

    public CompanyRatingColumnViewModel CompanyColumn => _companyColumn;

    public int? Score
    {
        get => _score.Score;
        set
        {
            if (_score.Score == value)
                return;

            _score.Score = value;

            OnPropertyChanged();
            _companyColumn.RefreshRating();
            _onScoreChanged();
        }
    }
}