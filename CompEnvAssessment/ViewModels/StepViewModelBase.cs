using CommunityToolkit.Mvvm.ComponentModel;
using CompEnvAssessment.Models;

namespace CompEnvAssessment.ViewModels;

public abstract partial class StepViewModelBase : ViewModelBase
{
    protected StepViewModelBase(
        AppProject project,
        int order,
        string title,
        string helpTitle,
        string helpText)
    {
        Project = project;
        Order = order;
        Title = title;
        HelpTitle = helpTitle;
        HelpText = helpText;
    }

    protected AppProject Project { get; }

    public int Order { get; }

    public string Title { get; }

    public string HelpTitle { get; }

    public string HelpText { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isUnlocked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isCompleted;

    [ObservableProperty]
    private bool isCurrent;

    public string StatusText
    {
        get
        {
            if (IsCompleted)
                return "Готово";

            if (IsUnlocked)
                return "Требует заполнения";

            return "Недоступно";
        }
    }

    public abstract StepValidationResult Validate();

    public virtual void OnEnter()
    {
    }

    public virtual void OnLeave()
    {
    }

    public StepValidationResult RefreshCompletion()
    {
        StepValidationResult result = Validate();
        IsCompleted = result.IsValid;
        return result;
    }
}