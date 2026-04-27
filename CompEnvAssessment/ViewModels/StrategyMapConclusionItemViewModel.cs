namespace CompEnvAssessment.ViewModels;

public class StrategyMapConclusionItemViewModel : ViewModelBase
{
    public StrategyMapConclusionItemViewModel(
        string title,
        string text)
    {
        Title = title;
        Text = text;
    }

    public string Title { get; }

    public string Text { get; }

    public bool HasText => !string.IsNullOrWhiteSpace(Text);

    public string DisplayText
    {
        get
        {
            if (HasText)
                return Text;

            return "Вывод не заполнен.";
        }
    }

    public string StatusText
    {
        get
        {
            if (HasText)
                return "Вывод заполнен";

            return "Не заполнено";
        }
    }
}
