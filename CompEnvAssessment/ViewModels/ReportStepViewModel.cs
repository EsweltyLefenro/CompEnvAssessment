using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Models;
using CompEnvAssessment.Services;
using System.IO;

namespace CompEnvAssessment.ViewModels;

public partial class ReportStepViewModel : StepViewModelBase
{
    private readonly ValidationService _validationService;
    private readonly RatingCalculationService _ratingCalculationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly ReportService _reportService;
    private readonly FileDialogService _fileDialogService;

    public ReportStepViewModel(
        AppProject project,
        ValidationService validationService,
        RatingCalculationService ratingCalculationService,
        StrategyMapService strategyMapService,
        ReportService reportService,
        FileDialogService fileDialogService)
        : base(
            project,
            5,
            "Отчёт",
            "Как сформировать отчёт",
            "Проверьте готовность данных и сформируйте итоговый PDF-отчёт.")
    {
        _validationService = validationService;
        _ratingCalculationService = ratingCalculationService;
        _strategyMapService = strategyMapService;
        _reportService = reportService;
        _fileDialogService = fileDialogService;
    }

    public ObservableCollection<ReportCheckItemViewModel> Checks { get; } = new();

    public ObservableCollection<ReportPreviewSectionViewModel> PreviewSections { get; } = new();

    [ObservableProperty]
    private string reportMessage = string.Empty;

    public bool IsReportReady =>
        Checks.Count > 0 && Checks.All(check => check.IsPassed);

    public string ReportStatusText =>
        IsReportReady
            ? "Данные готовы для формирования отчёта."
            : "Для формирования отчёта необходимо исправить замечания.";

    public override void OnEnter()
    {
        RefreshReportState();
    }

    public override StepValidationResult Validate()
    {
        IReadOnlyList<string> errors = _validationService.ValidateReport(Project);

        if (errors.Count == 0)
            return StepValidationResult.Success();

        return StepValidationResult.Fail(errors);
    }

    [RelayCommand]
    private void RefreshReportState()
    {
        Checks.Clear();

        Checks.Add(new ReportCheckItemViewModel(
            "Начальные данные",
            "ФИО, группа, режим работы, название и описание предприятия.",
            _validationService.ValidateStartup(Project)));

        Checks.Add(new ReportCheckItemViewModel(
            "Факторы и веса",
            "Добавлены факторы конкурентоспособности, веса заполнены, сумма весов равна 1,00.",
            _validationService.ValidateFactors(Project)));

        Checks.Add(new ReportCheckItemViewModel(
            "Оценка компаний",
            "Заполнены доли рынка и оценки всех компаний по всем факторам.",
            _validationService.ValidateCompanyRatings(Project)));

        Checks.Add(new ReportCheckItemViewModel(
            "Карты стратегической группировки",
            "Все пары факторов обработаны, минимум три карты имеют текстовый вывод.",
            _validationService.ValidateStrategyMaps(Project)));

        Checks.Add(new ReportCheckItemViewModel(
            "Итоговый вывод",
            "Сформулирован общий стратегический вывод по результатам анализа.",
            _validationService.ValidateSummary(Project)));

        RebuildPreviewSections();

        OnPropertyChanged(nameof(IsReportReady));
        OnPropertyChanged(nameof(ReportStatusText));

        BuildReportCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanBuildReport))]
    private void BuildReport()
    {
        RefreshReportState();

        if (!IsReportReady)
        {
            ReportMessage = "Отчёт нельзя сформировать: есть незавершённые разделы.";
            return;
        }

        string defaultFileName = BuildDefaultFileName();
        string? filePath = _fileDialogService.GetPdfSavePath(defaultFileName);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            ReportMessage = "Сохранение отчёта отменено.";
            return;
        }

        try
        {
            _reportService.GeneratePdf(Project, filePath);
            ReportMessage = $"PDF-отчёт сохранён: {filePath}";
        }
        catch (Exception ex)
        {
            ReportMessage = $"Не удалось сформировать PDF-отчёт: {ex.Message}";
        }
    }

    private bool CanBuildReport()
    {
        return IsReportReady;
    }

    private void RebuildPreviewSections()
    {
        PreviewSections.Clear();

        PreviewSections.Add(new ReportPreviewSectionViewModel(
            "1. Начальные данные",
            BuildInitialDataPreview()));

        PreviewSections.Add(new ReportPreviewSectionViewModel(
            "2. Факторы конкурентоспособности",
            BuildFactorsPreview()));

        PreviewSections.Add(new ReportPreviewSectionViewModel(
            "3. Оценка компаний",
            BuildCompaniesPreview()));

        PreviewSections.Add(new ReportPreviewSectionViewModel(
            "4. Карты стратегической группировки",
            BuildStrategyMapsPreview()));

        PreviewSections.Add(new ReportPreviewSectionViewModel(
            "5. Итоговый вывод",
            string.IsNullOrWhiteSpace(Project.Summary)
                ? "Итоговый вывод пока не заполнен."
                : Project.Summary));
    }

    private string BuildInitialDataPreview()
    {
        string mode = Project.UserProfile.Mode == UserMode.Teacher
            ? "Преподаватель"
            : "Студент";

        return
            $"ФИО: {GetValueOrPlaceholder(Project.UserProfile.FullName)}{Environment.NewLine}" +
            $"Группа: {GetValueOrPlaceholder(Project.UserProfile.Group)}{Environment.NewLine}" +
            $"Режим: {mode}{Environment.NewLine}" +
            $"Предприятие: {GetValueOrPlaceholder(Project.EnterpriseProfile.Name)}{Environment.NewLine}" +
            $"Описание: {GetValueOrPlaceholder(Project.EnterpriseProfile.Description)}";
    }

    private string BuildFactorsPreview()
    {
        if (Project.Factors.Count == 0)
            return "Факторы пока не добавлены.";

        return string.Join(
            Environment.NewLine,
            Project.Factors.Select((factor, index) =>
                $"{index + 1}. {GetValueOrPlaceholder(factor.Name)} — вес {factor.Weight:0.00}"));
    }

    private string BuildCompaniesPreview()
    {
        if (Project.Companies.Count == 0)
            return "Компании пока не добавлены.";

        return string.Join(
            Environment.NewLine,
            Project.Companies.Select(company =>
            {
                decimal rating = _ratingCalculationService.CalculateWeightedRating(
                    company,
                    Project.Factors);

                return $"{company.Name}: доля рынка {company.MarketShare:0.00}%, рейтинг {rating:0.00}";
            }));
    }

    private string BuildStrategyMapsPreview()
    {
        if (Project.StrategyMapPairs.Count == 0)
            return "Карты стратегической группировки пока не сформированы.";

        return string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            Project.StrategyMapPairs.Select((pair, index) =>
            {
                string title = _strategyMapService.GetPairTitle(Project, pair);
                string text = GetStrategyMapConclusionText(pair);

                return $"{index + 1}. {title}{Environment.NewLine}{text}";
            }));
    }

    private string BuildDefaultFileName()
    {
        string enterpriseName = string.IsNullOrWhiteSpace(Project.EnterpriseProfile.Name)
            ? "report"
            : Project.EnterpriseProfile.Name;

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            enterpriseName = enterpriseName.Replace(invalidChar, '_');
        }

        return $"{enterpriseName}_competitive_assessment.pdf";
    }

    private static string GetStrategyMapConclusionText(StrategyMapPair pair)
    {
        if (pair.HasTextConclusion)
            return pair.Conclusion;

        if (pair.IsSkipped)
            return "По данной паре факторов вывод не требуется.";

        return "Вывод не заполнен.";
    }

    private static string GetValueOrPlaceholder(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "не указано"
            : value;
    }
}