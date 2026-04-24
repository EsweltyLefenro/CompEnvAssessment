using CompEnvAssessment.Models;

namespace CompEnvAssessment.Services;

public class DemoDataService
{
    private readonly StrategyMapService _strategyMapService;

    public DemoDataService(StrategyMapService strategyMapService)
    {
        _strategyMapService = strategyMapService;
    }

    public void FillTeacherDemoProject(AppProject project)
    {
        project.UserProfile.FullName = "Преподаватель";
        project.UserProfile.Group = "Демонстрационный режим";
        project.UserProfile.Mode = UserMode.Teacher;

        project.EnterpriseProfile.Name = "АльфаСофт";
        project.EnterpriseProfile.Description =
            "Учебное IT-предприятие, разрабатывающее программные решения для автоматизации управленческих процессов.";

        project.Factors.Clear();

        project.Factors.Add(new CompetitiveFactor
        {
            Name = "Функциональность продукта",
            Weight = 0.25m
        });

        project.Factors.Add(new CompetitiveFactor
        {
            Name = "Стоимость внедрения",
            Weight = 0.20m
        });

        project.Factors.Add(new CompetitiveFactor
        {
            Name = "Качество технической поддержки",
            Weight = 0.20m
        });

        project.Factors.Add(new CompetitiveFactor
        {
            Name = "Интеграция с внешними системами",
            Weight = 0.20m
        });

        project.Factors.Add(new CompetitiveFactor
        {
            Name = "Репутация на рынке",
            Weight = 0.15m
        });

        project.InitializeDefaultCompanies();

        Company userCompany = project.Companies.First(company => company.IsUserCompany);
        userCompany.Name = "Вы";
        userCompany.MarketShare = 18;

        Company company1 = project.Companies.First(company => company.Name == "Компания 1");
        company1.MarketShare = 24;

        Company company2 = project.Companies.First(company => company.Name == "Компания 2");
        company2.MarketShare = 31;

        Company company3 = project.Companies.First(company => company.Name == "Компания 3");
        company3.MarketShare = 15;

        FillScores(project, userCompany, 8, 6, 7, 7, 6);
        FillScores(project, company1, 7, 8, 6, 5, 7);
        FillScores(project, company2, 9, 5, 8, 8, 8);
        FillScores(project, company3, 6, 9, 5, 6, 5);

        _strategyMapService.RebuildPairs(project);

        for (int i = 0; i < project.StrategyMapPairs.Count; i++)
        {
            StrategyMapPair pair = project.StrategyMapPairs[i];

            if (i < AppConstants.MinTextStrategyMapConclusions)
            {
                pair.Conclusion =
                    "По данной карте видно положение предприятия относительно конкурентов. " +
                    "Для усиления конкурентоспособности целесообразно улучшить слабый фактор без потери текущих преимуществ.";
            }
            else
            {
                pair.IsSkipped = true;
            }
        }

        project.Summary =
            "Предприятие занимает устойчивую конкурентную позицию, однако уступает отдельным конкурентам по ряду факторов. " +
            "Основными направлениями развития являются повышение качества поддержки, усиление интеграционных возможностей " +
            "и формирование более заметной рыночной репутации.";
    }

    private static void FillScores(AppProject project, Company company, params int[] scores)
    {
        for (int i = 0; i < project.Factors.Count && i < scores.Length; i++)
        {
            Guid factorId = project.Factors[i].Id;

            CompanyScore? score = company.Scores.FirstOrDefault(item => item.FactorId == factorId);

            if (score is not null)
            {
                score.Score = scores[i];
            }
        }
    }
}