using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class InitialDataViewModel : ViewModelBase
{
    private readonly AppProject _project;
    private readonly ValidationService _validationService;
    private readonly DemoDataService _demoDataService;
    private readonly Action _confirmInitialData;
    private readonly Action _startEditingInitialData;

    public InitialDataViewModel(
        AppProject project,
        ValidationService validationService,
        DemoDataService demoDataService,
        Action confirmInitialData,
        Action startEditingInitialData)
    {
        _project = project;
        _validationService = validationService;
        _demoDataService = demoDataService;
        _confirmInitialData = confirmInitialData;
        _startEditingInitialData = startEditingInitialData;

        IsEditing = true;
    }

    public UserProfile UserProfile => _project.UserProfile;

    public EnterpriseProfile EnterpriseProfile => _project.EnterpriseProfile;

    public ObservableCollection<string> Errors { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsReadOnly))]
    [NotifyPropertyChangedFor(nameof(ActionButtonText))]
    [NotifyPropertyChangedFor(nameof(CanEditInitialData))]
    private bool isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditInitialData))]
    [NotifyPropertyChangedFor(nameof(ActionButtonText))]
    private bool isConfirmed;

    [ObservableProperty]
    private bool isTeacherModeEnabled = true;

    public bool IsReadOnly => !IsEditing;

    public bool CanEditInitialData => IsConfirmed && !IsEditing;

    public string ActionButtonText =>
        IsConfirmed ? "Применить изменения" : "Начать работу";

    public bool IsStudentModeSelected => UserProfile.Mode == UserMode.Student;

    public bool IsTeacherModeSelected => UserProfile.Mode == UserMode.Teacher;

    [RelayCommand]
    private void SelectStudentMode()
    {
        if (!IsEditing)
            return;

        UserProfile.Mode = UserMode.Student;

        OnPropertyChanged(nameof(IsStudentModeSelected));
        OnPropertyChanged(nameof(IsTeacherModeSelected));
    }

    [RelayCommand(CanExecute = nameof(CanSelectTeacherMode))]
    private void SelectTeacherMode()
    {
        if (!IsEditing)
            return;

        _demoDataService.FillTeacherDemoProject(_project);

        OnPropertyChanged(nameof(IsStudentModeSelected));
        OnPropertyChanged(nameof(IsTeacherModeSelected));
    }

    private bool CanSelectTeacherMode()
    {
        return IsTeacherModeEnabled;
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private void EditInitialData()
    {
        Errors.Clear();

        IsEditing = true;
        _startEditingInitialData();
    }

    private bool CanEdit()
    {
        return CanEditInitialData;
    }

    [RelayCommand(CanExecute = nameof(CanApplyInitialData))]
    private void ApplyInitialData()
    {
        Errors.Clear();

        IReadOnlyList<string> errors = _validationService.ValidateStartup(_project);

        foreach (string error in errors)
        {
            Errors.Add(error);
        }

        if (errors.Count > 0)
            return;

        if (_project.Companies.Count == 0)
        {
            _project.InitializeDefaultCompanies();
        }

        _project.SyncUserCompanyName();

        IsConfirmed = true;
        IsEditing = false;

        _confirmInitialData();
    }

    private bool CanApplyInitialData()
    {
        return IsEditing;
    }

    partial void OnIsEditingChanged(bool value)
    {
        EditInitialDataCommand.NotifyCanExecuteChanged();
        ApplyInitialDataCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsConfirmedChanged(bool value)
    {
        EditInitialDataCommand.NotifyCanExecuteChanged();
        ApplyInitialDataCommand.NotifyCanExecuteChanged();
    }
}
