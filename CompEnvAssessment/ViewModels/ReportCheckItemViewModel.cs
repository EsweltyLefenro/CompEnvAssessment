namespace CompEnvAssessment.ViewModels;

public class ReportCheckItemViewModel : ViewModelBase
{
    public ReportCheckItemViewModel(
        string title,
        string description,
        IEnumerable<string> errors)
    {
        Title = title;
        Description = description;
        Errors = errors.ToList();
    }

    public string Title { get; }

    public string Description { get; }

    public IReadOnlyList<string> Errors { get; }

    public bool IsPassed => Errors.Count == 0;

    public string StatusMark => IsPassed ? "✓" : "×";

    public string StatusText => IsPassed ? "Готово" : "Требует исправления";
}