namespace CompEnvAssessment.ViewModels;

public class ReportPreviewSectionViewModel : ViewModelBase
{
    public ReportPreviewSectionViewModel(string title, string body)
    {
        Title = title;
        Body = body;
    }

    public string Title { get; }

    public string Body { get; }
}