using CompEnvAssessment.Models;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly AppProject _project;
    private readonly ValidationService _validationService;
    private readonly RatingCalculationService _ratingCalculationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly DemoDataService _demoDataService;
    private readonly ReportService _reportService;
    private readonly FileDialogService _fileDialogService;

    public MainViewModel()
    {
        _project = new AppProject();

        _validationService = new ValidationService();
        _ratingCalculationService = new RatingCalculationService();
        _strategyMapService = new StrategyMapService();
        _demoDataService = new DemoDataService(_strategyMapService);
        _reportService = new ReportService(
            _ratingCalculationService,
            _strategyMapService);
        _fileDialogService = new FileDialogService();

        Workspace = new WorkspaceViewModel(
            _project,
            _validationService,
            _ratingCalculationService,
            _strategyMapService,
            _reportService,
            _fileDialogService);

        Workspace.SetWorkspaceEnabled(false);

        InitialData = new InitialDataViewModel(
            _project,
            _validationService,
            _demoDataService,
            ConfirmInitialData,
            StartEditingInitialData);
    }

    public InitialDataViewModel InitialData { get; }

    public WorkspaceViewModel Workspace { get; }

    private void ConfirmInitialData()
    {
        Workspace.SetWorkspaceEnabled(true);
        Workspace.RefreshCurrentStep();
    }

    private void StartEditingInitialData()
    {
        Workspace.SetWorkspaceEnabled(false);
    }
}
