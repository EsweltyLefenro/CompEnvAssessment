using CommunityToolkit.Mvvm.ComponentModel;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class SummaryStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;
    private readonly Action _refreshWorkflow;

    public SummaryStepViewModel(
        AppProject project,
        ValidationService validationService,
        Action refreshWorkflow)
        : base(
            project,
            4,
            "Итоги",
            "Как сформулировать итог",
            "Опишите общую конкурентную позицию предприятия, его сильные и слабые стороны, а также рекомендуемые направления развития.")
    {
        _validationService = validationService;
        _refreshWorkflow = refreshWorkflow;
    }

    public string Summary
    {
        get => Project.Summary;
        set
        {
            if (Project.Summary == value)
                return;

            Project.Summary = value;
            OnPropertyChanged();
            _refreshWorkflow();
        }
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateSummary(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }
}