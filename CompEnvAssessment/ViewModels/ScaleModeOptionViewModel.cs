using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.ViewModels;

public partial class ScaleModeOptionViewModel : ViewModelBase
{
    public ScaleModeOptionViewModel(
        string label,
        double scale)
    {
        Label = label;
        Scale = scale;
    }

    public string Label { get; }

    public double Scale { get; }

    [ObservableProperty]
    private bool isSelected;
}
