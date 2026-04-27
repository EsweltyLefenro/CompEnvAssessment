using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public partial class EnterpriseProfile : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Description);
}
