using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public class StrategyMapPointViewModel : ViewModelBase
{
    private const double MapSize = 440;
    private const double MinScore = 1;
    private const double MaxScore = 10;

    public StrategyMapPointViewModel(StrategyMapPoint point)
    {
        Point = point;
    }

    public StrategyMapPoint Point { get; }

    public string CompanyName => Point.CompanyName;

    public decimal X => Point.X;

    public decimal Y => Point.Y;

    public decimal MarketShare => Point.MarketShare;

    public bool IsUserCompany => Point.IsUserCompany;

    public string Label =>
        $"{CompanyName}: X={X:0}, Y={Y:0}, доля рынка {MarketShare:0.##}%";

    public double Size
    {
        get
        {
            double size = 18 + (double)MarketShare * 0.7;

            if (size < 18)
                return 18;

            if (size > 64)
                return 64;

            return size;
        }
    }

    public double Left
    {
        get
        {
            double normalized = ((double)X - MinScore) / (MaxScore - MinScore);
            return normalized * MapSize - Size / 2;
        }
    }

    public double Top
    {
        get
        {
            double normalized = ((double)Y - MinScore) / (MaxScore - MinScore);
            return MapSize - normalized * MapSize - Size / 2;
        }
    }
}