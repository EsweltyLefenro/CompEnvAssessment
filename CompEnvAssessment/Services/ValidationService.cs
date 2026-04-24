using CompEnvAssessment.Models;

namespace CompEnvAssessment.Services;

public class ValidationService
{
    public IReadOnlyList<string> ValidateStartup(AppProject project)
    {
        List<string> errors = new();

        if (string.IsNullOrWhiteSpace(project.UserProfile.FullName))
            errors.Add("Укажите фамилию и имя.");

        if (string.IsNullOrWhiteSpace(project.UserProfile.Group))
            errors.Add("Укажите учебную группу.");

        if (string.IsNullOrWhiteSpace(project.EnterpriseProfile.Name))
            errors.Add("Укажите название предприятия.");

        if (string.IsNullOrWhiteSpace(project.EnterpriseProfile.Description))
            errors.Add("Укажите краткое описание предприятия.");

        return errors;
    }

    public IReadOnlyList<string> ValidateFactors(AppProject project)
    {
        List<string> errors = new();

        if (project.Factors.Count < AppConstants.MinFactorsCount)
        {
            errors.Add($"Добавьте минимум {AppConstants.MinFactorsCount} фактора.");
        }

        if (project.Factors.Count > AppConstants.MaxFactorsCount)
        {
            errors.Add($"Количество факторов не должно превышать {AppConstants.MaxFactorsCount}.");
        }

        for (int i = 0; i < project.Factors.Count; i++)
        {
            CompetitiveFactor factor = project.Factors[i];

            if (string.IsNullOrWhiteSpace(factor.Name))
            {
                errors.Add($"У фактора №{i + 1} не указано название.");
            }

            if (factor.Weight <= 0)
            {
                errors.Add($"У фактора «{GetFactorDisplayName(factor, i)}» вес должен быть больше 0,00.");
            }

            if (factor.Weight >= 1)
            {
                errors.Add($"У фактора «{GetFactorDisplayName(factor, i)}» вес должен быть меньше 1,00.");
            }
        }

        if (project.TotalWeight != AppConstants.RequiredWeightSum)
        {
            errors.Add($"Сумма весов должна быть равна 1,00. Сейчас: {project.TotalWeight:0.00}.");
        }

        return errors;
    }

    public IReadOnlyList<string> ValidateCompanyRatings(AppProject project)
    {
        List<string> errors = new();

        errors.AddRange(ValidateFactors(project));

        Company? userCompany = project.Companies.FirstOrDefault(company => company.IsUserCompany);

        if (userCompany is null)
        {
            errors.Add("Не найдена собственная компания пользователя.");
        }

        int competitorsCount = project.Companies.Count(company => !company.IsUserCompany);

        if (competitorsCount < AppConstants.MinCompetitorsCount)
        {
            errors.Add($"Должно быть минимум {AppConstants.MinCompetitorsCount} конкурента.");
        }

        decimal totalMarketShare = project.Companies.Sum(company => company.MarketShare);

        if (totalMarketShare > AppConstants.MaxMarketShare)
        {
            errors.Add($"Суммарная доля рынка не должна превышать 100%. Сейчас: {totalMarketShare:0.00}%.");
        }

        foreach (Company company in project.Companies)
        {
            if (string.IsNullOrWhiteSpace(company.Name))
            {
                errors.Add("У одной из компаний не указано название.");
            }

            foreach (CompetitiveFactor factor in project.Factors)
            {
                CompanyScore? score = company.Scores.FirstOrDefault(item => item.FactorId == factor.Id);

                if (score is null || !score.IsFilled)
                {
                    errors.Add($"Для компании «{company.Name}» не указана оценка по фактору «{factor.Name}».");
                }
            }
        }

        return errors;
    }

    public IReadOnlyList<string> ValidateStrategyMaps(AppProject project)
    {
        List<string> errors = new();

        errors.AddRange(ValidateCompanyRatings(project));

        int expectedPairsCount = GetExpectedStrategyMapPairsCount(project);

        if (project.StrategyMapPairs.Count != expectedPairsCount)
        {
            errors.Add("Список карт стратегической группировки нужно обновить.");
            return errors;
        }

        foreach (StrategyMapPair pair in project.StrategyMapPairs)
        {
            if (!pair.IsCompleted)
            {
                errors.Add("Не все пары факторов обработаны.");
                break;
            }
        }

        if (project.TextStrategyMapConclusionsCount < AppConstants.MinTextStrategyMapConclusions)
        {
            errors.Add($"Минимум {AppConstants.MinTextStrategyMapConclusions} карты должны иметь текстовый вывод.");
        }

        return errors;
    }

    public IReadOnlyList<string> ValidateSummary(AppProject project)
    {
        List<string> errors = new();

        errors.AddRange(ValidateStrategyMaps(project));

        if (string.IsNullOrWhiteSpace(project.Summary))
        {
            errors.Add("Заполните итоговый вывод.");
        }

        return errors;
    }

    public IReadOnlyList<string> ValidateReport(AppProject project)
    {
        return ValidateSummary(project);
    }

    public int GetExpectedStrategyMapPairsCount(AppProject project)
    {
        int factorsCount = project.Factors.Count;

        if (factorsCount < 2)
            return 0;

        return factorsCount * (factorsCount - 1) / 2;
    }

    private static string GetFactorDisplayName(CompetitiveFactor factor, int index)
    {
        if (!string.IsNullOrWhiteSpace(factor.Name))
            return factor.Name;

        return $"Фактор №{index + 1}";
    }
}