using Microsoft.Win32;

namespace CompEnvAssessment.Services;

public class FileDialogService
{
    public string? GetPdfSavePath(string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Сохранить PDF-отчёт",
            Filter = "PDF-файл (*.pdf)|*.pdf",
            FileName = defaultFileName,
            AddExtension = true,
            DefaultExt = ".pdf"
        };

        bool? result = dialog.ShowDialog();

        return result == true
            ? dialog.FileName
            : null;
    }
}