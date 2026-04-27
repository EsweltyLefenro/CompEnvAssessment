using CommunityToolkit.Mvvm.ComponentModel;

namespace CompEnvAssessment.Models;

public partial class UserProfile : ObservableObject
{
    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string group = string.Empty;

    [ObservableProperty]
    private UserMode mode = UserMode.Student;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(FullName)
        && !string.IsNullOrWhiteSpace(Group);
}
