using System.Windows;
using CompEnvAssessment.ViewModels;

namespace CompEnvAssessment;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}