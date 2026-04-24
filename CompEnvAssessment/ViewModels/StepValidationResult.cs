namespace CompEnvAssessment.ViewModels;

public class StepValidationResult
{
    public StepValidationResult(bool isValid, IEnumerable<string> errors)
    {
        IsValid = isValid;
        Errors = errors.ToList();
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }

    public static StepValidationResult Success()
    {
        return new StepValidationResult(true, Array.Empty<string>());
    }

    public static StepValidationResult Fail(params string[] errors)
    {
        return new StepValidationResult(false, errors);
    }

    public static StepValidationResult Fail(IEnumerable<string> errors)
    {
        return new StepValidationResult(false, errors);
    }
}