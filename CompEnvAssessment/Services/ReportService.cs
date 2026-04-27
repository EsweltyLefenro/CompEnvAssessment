using CompEnvAssessment.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CompEnvAssessment.Services;

public class ReportService
{
    private readonly RatingCalculationService _ratingCalculationService;
    private readonly StrategyMapService _strategyMapService;

    public ReportService(
        RatingCalculationService ratingCalculationService,
        StrategyMapService strategyMapService)
    {
        _ratingCalculationService = ratingCalculationService;
        _strategyMapService = strategyMapService;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public void GeneratePdf(AppProject project, string filePath)
    {
        QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);

                page.DefaultTextStyle(text => text
                    .FontSize(11)
                    .FontFamily("Arial"));

                page.Header()
                    .Text(project.EnterpriseProfile.Name)
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken2);

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Страница ");
                        text.CurrentPageNumber();
                    });

                page.Content().Column(column =>
                {
                    column.Spacing(18);

                    BuildTitleBlock(column, project);
                    BuildFactorsSection(column, project);
                    BuildCompaniesSection(column, project);
                    BuildStrategyMapsSection(column, project);
                    BuildSummarySection(column, project);
                });
            });
        }).GeneratePdf(filePath);
    }

    private static void BuildTitleBlock(ColumnDescriptor column, AppProject project)
    {
        string mode = project.UserProfile.Mode == UserMode.Teacher
            ? "Преподаватель"
            : "Студент";

        column.Item().Column(title =>
        {
            title.Spacing(8);

            title.Item()
                .Text("Оценка конкурентоспособности предприятия во внешней среде")
                .FontSize(20)
                .Bold()
                .FontColor(Colors.Blue.Darken4);

            title.Item()
                .Text(project.EnterpriseProfile.Name)
                .FontSize(18)
                .Bold();

            title.Item()
                .PaddingTop(6)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell().Text($"ФИО: {project.UserProfile.FullName}");
                    table.Cell().Text($"Группа: {project.UserProfile.Group}");
                    table.Cell().Text($"Режим: {mode}");
                    table.Cell().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                });

            title.Item()
                .PaddingTop(8)
                .Text("Описание предприятия")
                .Bold();

            title.Item().Text(project.EnterpriseProfile.Description);
        });

        column.Item()
            .LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);
    }

    private static void BuildFactorsSection(ColumnDescriptor column, AppProject project)
    {
        column.Item()
            .Text("1. Факторы конкурентоспособности")
            .FontSize(15)
            .Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(34);
                columns.RelativeColumn();
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "№");
                HeaderCell(header.Cell(), "Фактор");
                HeaderCell(header.Cell(), "Вес");
            });

            for (int i = 0; i < project.Factors.Count; i++)
            {
                CompetitiveFactor factor = project.Factors[i];

                BodyCell(table.Cell(), (i + 1).ToString());
                BodyCell(table.Cell(), factor.Name);
                BodyCell(table.Cell(), factor.Weight.ToString("0.00"));
            }
        });
    }

    private void BuildCompaniesSection(ColumnDescriptor column, AppProject project)
    {
        column.Item()
            .Text("2. Оценка компаний")
            .FontSize(15)
            .Bold();

        column.Item()
            .Text("В таблице представлены доли рынка и итоговые взвешенные рейтинги компаний.")
            .FontColor(Colors.Grey.Darken2);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.ConstantColumn(90);
                columns.ConstantColumn(90);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Компания");
                HeaderCell(header.Cell(), "Доля рынка");
                HeaderCell(header.Cell(), "Рейтинг");
            });

            foreach (Company company in project.Companies)
            {
                decimal rating = _ratingCalculationService.CalculateWeightedRating(
                    company,
                    project.Factors);

                BodyCell(table.Cell(), company.Name);
                BodyCell(table.Cell(), $"{company.MarketShare:0.00}%");
                BodyCell(table.Cell(), rating.ToString("0.00"));
            }
        });

        column.Item()
            .PaddingTop(8)
            .Text("Оценки по факторам")
            .Bold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.ConstantColumn(60);

                foreach (Company _ in project.Companies)
                {
                    columns.RelativeColumn();
                }
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Фактор");
                HeaderCell(header.Cell(), "Вес");

                foreach (Company company in project.Companies)
                {
                    HeaderCell(header.Cell(), company.Name);
                }
            });

            foreach (CompetitiveFactor factor in project.Factors)
            {
                BodyCell(table.Cell(), factor.Name);
                BodyCell(table.Cell(), factor.Weight.ToString("0.00"));

                foreach (Company company in project.Companies)
                {
                    CompanyScore? score = company.Scores
                        .FirstOrDefault(item => item.FactorId == factor.Id);

                    BodyCell(table.Cell(), score?.Score?.ToString() ?? "-");
                }
            }
        });
    }

    private void BuildStrategyMapsSection(ColumnDescriptor column, AppProject project)
    {
        column.Item()
            .Text("3. Карты стратегической группировки")
            .FontSize(15)
            .Bold();

        if (project.StrategyMapPairs.Count == 0)
        {
            column.Item().Text("Карты стратегической группировки не сформированы.");
            return;
        }

        for (int i = 0; i < project.StrategyMapPairs.Count; i++)
        {
            StrategyMapPair pair = project.StrategyMapPairs[i];
            string title = _strategyMapService.GetPairTitle(project, pair);

            column.Item()
                .PaddingTop(6)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(10)
                .Column(block =>
                {
                    block.Spacing(5);

                    block.Item()
                        .Text($"{i + 1}. {title}")
                        .Bold();

                    if (pair.HasTextConclusion)
                    {
                        block.Item().Text(pair.Conclusion);
                    }
                    else if (pair.IsSkipped)
                    {
                        block.Item()
                            .Text("По данной паре факторов вывод не требуется.")
                            .Italic()
                            .FontColor(Colors.Grey.Darken2);
                    }
                    else
                    {
                        block.Item()
                            .Text("Вывод не заполнен.")
                            .FontColor(Colors.Red.Darken2);
                    }
                });
        }
    }

    private static void BuildSummarySection(ColumnDescriptor column, AppProject project)
    {
        column.Item()
            .Text("4. Итоговый стратегический вывод")
            .FontSize(15)
            .Bold();

        column.Item().Text(project.Summary);
    }

    private static void HeaderCell(IContainer container, string text)
    {
        container
            .Background(Colors.Blue.Darken4)
            .Border(1)
            .BorderColor(Colors.Blue.Darken4)
            .Padding(5)
            .Text(text)
            .FontColor(Colors.White)
            .Bold();
    }

    private static void BodyCell(IContainer container, string text)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(5)
            .Text(text);
    }
}
