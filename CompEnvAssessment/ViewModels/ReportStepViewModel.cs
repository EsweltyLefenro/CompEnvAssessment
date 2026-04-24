using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class ReportStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;

    public ReportStepViewModel(
        AppProject project,
        ValidationService validationService)
        : base(
            project,
            5,
            "Отчёт",
            "Как сформировать отчёт",
            "Проверьте готовность данных и сформируйте итоговый PDF-отчёт.")
    {
        _validationService = validationService;
    }

    public ObservableCollection<string> ReportErrors { get; } = new();

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateReport(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }

    [RelayCommand]
    private void CheckReportReadiness()
    {
        ReportErrors.Clear();

        IReadOnlyList<string> errors = _validationService.ValidateReport(Project);

        foreach (string error in errors)
        {
            ReportErrors.Add(error);
        }

        if (errors.Count == 0)
        {
            ReportErrors.Add("Данные готовы для формирования отчёта.");
        }
    }
}