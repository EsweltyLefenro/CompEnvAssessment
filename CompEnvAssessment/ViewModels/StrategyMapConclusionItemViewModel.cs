namespace CompEnvAssessment.ViewModels;

public class StrategyMapConclusionItemViewModel : ViewModelBase
{
    public StrategyMapConclusionItemViewModel(
        string title,
        string text,
        bool isSkipped)
    {
        Title = title;
        Text = text;
        IsSkipped = isSkipped;
    }

    public string Title { get; }

    public string Text { get; }

    public bool IsSkipped { get; }

    public bool HasText => !string.IsNullOrWhiteSpace(Text);

    public string DisplayText
    {
        get
        {
            if (HasText)
                return Text;

            if (IsSkipped)
                return "По данной паре факторов вывод не требуется.";

            return "Вывод не заполнен.";
        }
    }

    public string StatusText
    {
        get
        {
            if (HasText)
                return "Вывод заполнен";

            if (IsSkipped)
                return "Вывод не требуется";

            return "Не заполнено";
        }
    }
}