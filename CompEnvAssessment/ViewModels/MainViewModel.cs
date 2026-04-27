using CompEnvAssessment.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompEnvAssessment.Services;

namespace CompEnvAssessment.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const double HdScale = 0.50;
    private const double FullHdScale = 0.75;
    private const double TwoKScale = 1.00;
    private const double FourKScale = 1.50;

    private readonly AppProject _project;
    private readonly ValidationService _validationService;
    private readonly RatingCalculationService _ratingCalculationService;
    private readonly StrategyMapService _strategyMapService;
    private readonly DemoDataService _demoDataService;
    private readonly ReportService _reportService;
    private readonly FileDialogService _fileDialogService;

    public MainViewModel()
    {
        ScaleModes = new ObservableCollection<ScaleModeOptionViewModel>
        {
            new("720p", HdScale),
            new("Full HD", FullHdScale),
            new("2K", TwoKScale),
            new("4K", FourKScale)
        };

        SelectScaleMode(ScaleModes[1]);

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

    public ObservableCollection<ScaleModeOptionViewModel> ScaleModes { get; }

    [ObservableProperty]
    private double appScale = FullHdScale;

    [RelayCommand]
    private void SelectScaleMode(ScaleModeOptionViewModel? scaleMode)
    {
        if (scaleMode is null)
            return;

        foreach (ScaleModeOptionViewModel option in ScaleModes)
        {
            option.IsSelected = ReferenceEquals(option, scaleMode);
        }

        AppScale = scaleMode.Scale;
    }

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
