using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class WorkspaceViewModel : ViewModelBase
{
    private readonly AppProject _project;

    public WorkspaceViewModel(
        AppProject project,
        ValidationService validationService,
        RatingCalculationService ratingCalculationService,
        StrategyMapService strategyMapService,
        ReportService reportService,
        FileDialogService fileDialogService)
    {
        _project = project;

        Steps = new ObservableCollection<StepViewModelBase>
        {
            new FactorsStepViewModel(
                project,
                validationService,
                strategyMapService,
                RefreshWorkflow),

            new CompanyRatingStepViewModel(
                project,
                validationService,
                ratingCalculationService,
                RefreshWorkflow),

            new StrategyMapsStepViewModel(
                project,
                validationService,
                strategyMapService,
                RefreshWorkflow),

            new SummaryStepViewModel(
                project,
                validationService,
                strategyMapService,
                RefreshWorkflow),

            new ReportStepViewModel(
                project,
                validationService,
                ratingCalculationService,
                strategyMapService,
                reportService,
                fileDialogService)
        };

        CurrentStep = Steps[0];
        CurrentStep.IsCurrent = true;

        RefreshWorkflow();
    }

    public ObservableCollection<StepViewModelBase> Steps { get; }

    [ObservableProperty]
    private StepViewModelBase currentStep = null!;

    [ObservableProperty]
    private bool isWorkspaceEnabled;

    public UserProfile UserProfile => _project.UserProfile;

    public EnterpriseProfile EnterpriseProfile => _project.EnterpriseProfile;

    public bool IsTeacherMode => _project.UserProfile.Mode == UserMode.Teacher;

    public bool IsWorkspaceBlocked => !IsWorkspaceEnabled;

    public void SetWorkspaceEnabled(bool value)
    {
        IsWorkspaceEnabled = value;
        OnPropertyChanged(nameof(IsWorkspaceBlocked));

        RefreshWorkflow();
    }

    public void RefreshCurrentStep()
    {
        if (!IsWorkspaceEnabled)
            return;

        CurrentStep.OnEnter();
        RefreshWorkflow();
    }

    [RelayCommand]
    private void SelectStep(StepViewModelBase? step)
    {
        if (!IsWorkspaceEnabled)
            return;

        if (step is null)
            return;

        if (!step.IsUnlocked)
            return;

        CurrentStep.OnLeave();
        CurrentStep.IsCurrent = false;

        CurrentStep = step;
        CurrentStep.IsCurrent = true;
        CurrentStep.OnEnter();

        RefreshWorkflow();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
    {
        if (!IsWorkspaceEnabled)
            return;

        int currentIndex = Steps.IndexOf(CurrentStep);

        if (currentIndex < 0 || currentIndex >= Steps.Count - 1)
            return;

        StepViewModelBase nextStep = Steps[currentIndex + 1];

        if (nextStep.IsUnlocked)
        {
            SelectStep(nextStep);
        }
    }

    private bool CanGoNext()
    {
        if (!IsWorkspaceEnabled)
            return false;

        int currentIndex = Steps.IndexOf(CurrentStep);

        if (currentIndex < 0 || currentIndex >= Steps.Count - 1)
            return false;

        return Steps[currentIndex + 1].IsUnlocked;
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        if (!IsWorkspaceEnabled)
            return;

        int currentIndex = Steps.IndexOf(CurrentStep);

        if (currentIndex <= 0)
            return;

        SelectStep(Steps[currentIndex - 1]);
    }

    private bool CanGoBack()
    {
        if (!IsWorkspaceEnabled)
            return false;

        return Steps.IndexOf(CurrentStep) > 0;
    }

    public void RefreshWorkflow()
    {
        if (!IsWorkspaceEnabled)
        {
            foreach (StepViewModelBase step in Steps)
            {
                step.IsUnlocked = false;
            }

            RefreshCommands();
            return;
        }

        if (IsTeacherMode)
        {
            foreach (StepViewModelBase step in Steps)
            {
                step.IsUnlocked = true;
                step.RefreshCompletion();
            }

            RefreshCommands();
            return;
        }

        bool previousStepsAreValid = true;

        for (int i = 0; i < Steps.Count; i++)
        {
            StepViewModelBase step = Steps[i];

            if (i == 0)
            {
                step.IsUnlocked = true;
            }
            else
            {
                step.IsUnlocked = previousStepsAreValid;
            }

            StepValidationResult validationResult = step.RefreshCompletion();

            if (!validationResult.IsValid)
            {
                previousStepsAreValid = false;
            }
        }

        RefreshCommands();
    }

    private void RefreshCommands()
    {
        GoNextCommand.NotifyCanExecuteChanged();
        GoBackCommand.NotifyCanExecuteChanged();

        OnPropertyChanged(nameof(IsTeacherMode));
        OnPropertyChanged(nameof(IsWorkspaceBlocked));
    }
}
