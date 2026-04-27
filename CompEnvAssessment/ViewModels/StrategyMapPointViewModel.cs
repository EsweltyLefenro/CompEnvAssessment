using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public class StrategyMapPointViewModel : ViewModelBase
{
    private const double MapSize = 440;
    private const double CellSize = MapSize / 10;
    private const double MapLabelWidth = 120;
    private const double MaxPointDiameter = CellSize - 4;
    private const double MinPointDiameter = 6;
    private const double MinScore = 1;
    private const double MaxScore = 10;
    private readonly decimal _maxMarketShare;

    public StrategyMapPointViewModel(
        StrategyMapPoint point,
        decimal maxMarketShare)
    {
        Point = point;
        _maxMarketShare = maxMarketShare;
    }

    public StrategyMapPoint Point { get; }

    public string CompanyName => Point.CompanyName;

    public decimal X => Point.X;

    public decimal Y => Point.Y;

    public decimal MarketShare => Point.MarketShare;

    public bool IsUserCompany => Point.IsUserCompany;

    public string Label =>
        $"{CompanyName}: X={X:0}, Y={Y:0}, оцениваемая доля рынка {MarketShare:0.##}%";

    public double Size
    {
        get
        {
            if (_maxMarketShare <= 0 || MarketShare <= 0)
                return MinPointDiameter;

            double ratio = (double)(MarketShare / _maxMarketShare);
            double size = MaxPointDiameter * ratio;

            return Math.Clamp(size, MinPointDiameter, MaxPointDiameter);
        }
    }

    public double Left
    {
        get
        {
            double score = ClampScore((double)X);
            return (score - 0.5) * CellSize - Size / 2;
        }
    }

    public double Top
    {
        get
        {
            double score = ClampScore((double)Y);
            return MapSize - (score - 0.5) * CellSize - Size / 2;
        }
    }

    public double LabelWidth => MapLabelWidth;

    public double LabelLeft => Size / 2 - MapLabelWidth / 2;

    private static double ClampScore(double score)
    {
        if (score < MinScore)
            return MinScore;

        if (score > MaxScore)
            return MaxScore;

        return score;
    }
}
