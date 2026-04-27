using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CompEnvAssessment.Models;

namespace CompEnvAssessment.Behaviors;

public static class FactorWeightTextBoxBehavior
{
    private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(FactorWeightTextBoxBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element)
    {
        return (bool)element.GetValue(IsEnabledProperty);
    }

    public static void SetIsEnabled(DependencyObject element, bool value)
    {
        element.SetValue(IsEnabledProperty, value);
    }

    private static void OnIsEnabledChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not TextBox textBox)
        {
            return;
        }

        if ((bool)args.OldValue)
        {
            textBox.Loaded -= OnLoaded;
            textBox.DataContextChanged -= OnDataContextChanged;
            textBox.PreviewTextInput -= OnPreviewTextInput;
            textBox.PreviewKeyDown -= OnPreviewKeyDown;
            textBox.LostFocus -= OnLostFocus;
            DataObject.RemovePastingHandler(textBox, OnPaste);
        }

        if ((bool)args.NewValue)
        {
            textBox.Loaded += OnLoaded;
            textBox.DataContextChanged += OnDataContextChanged;
            textBox.PreviewTextInput += OnPreviewTextInput;
            textBox.PreviewKeyDown += OnPreviewKeyDown;
            textBox.LostFocus += OnLostFocus;
            DataObject.AddPastingHandler(textBox, OnPaste);
            RefreshText(textBox);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs args)
    {
        RefreshText((TextBox)sender);
    }

    private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
    {
        RefreshText((TextBox)sender);
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs args)
    {
        var textBox = (TextBox)sender;
        string input = args.Text.Replace('.', ',');

        if (input != args.Text)
        {
            args.Handled = true;

            if (IsValidInput(textBox, input))
            {
                InsertText(textBox, input);
            }

            return;
        }

        args.Handled = !IsValidInput(textBox, input);
    }

    private static void OnPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (args.Key is Key.Enter or Key.Return)
        {
            Commit((TextBox)sender);
            args.Handled = true;
        }
    }

    private static void OnLostFocus(object sender, RoutedEventArgs args)
    {
        Commit((TextBox)sender);
    }

    private static void OnPaste(object sender, DataObjectPastingEventArgs args)
    {
        var textBox = (TextBox)sender;

        if (!args.DataObject.GetDataPresent(DataFormats.Text))
        {
            args.CancelCommand();
            return;
        }

        string input = ((string)args.DataObject.GetData(DataFormats.Text)).Replace('.', ',');
        if (!IsValidInput(textBox, input))
        {
            args.CancelCommand();
            return;
        }

        args.CancelCommand();
        InsertText(textBox, input);
    }

    private static bool IsValidInput(TextBox textBox, string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        if (input.Any(character => !char.IsDigit(character) && character != ','))
        {
            return false;
        }

        string proposedText = textBox.Text
            .Remove(textBox.SelectionStart, textBox.SelectionLength)
            .Insert(textBox.SelectionStart, input);

        return IsValidWeightText(proposedText);
    }

    private static bool IsValidWeightText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        if (text.StartsWith(','))
        {
            return false;
        }

        int commaIndex = text.IndexOf(',');
        if (commaIndex != text.LastIndexOf(','))
        {
            return false;
        }

        string integerPart = commaIndex >= 0 ? text[..commaIndex] : text;
        string fractionalPart = commaIndex >= 0 ? text[(commaIndex + 1)..] : string.Empty;

        return integerPart.Length <= 3
            && fractionalPart.Length <= 2;
    }

    private static void InsertText(TextBox textBox, string input)
    {
        int selectionStart = textBox.SelectionStart;
        textBox.Text = textBox.Text
            .Remove(selectionStart, textBox.SelectionLength)
            .Insert(selectionStart, input);
        textBox.CaretIndex = selectionStart + input.Length;
    }

    private static void Commit(TextBox textBox)
    {
        if (textBox.DataContext is not CompetitiveFactor factor)
        {
            return;
        }

        string text = textBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            factor.Weight = 0;
            textBox.Text = string.Empty;
            return;
        }

        if (!decimal.TryParse(text, NumberStyles.Number, RuCulture, out decimal weight))
        {
            factor.Weight = 0;
            textBox.Text = string.Empty;
            return;
        }

        factor.Weight = weight;
        textBox.Text = FormatWeight(factor.Weight);
        textBox.CaretIndex = textBox.Text.Length;
    }

    private static void RefreshText(TextBox textBox)
    {
        if (textBox.DataContext is not CompetitiveFactor factor)
        {
            textBox.Text = string.Empty;
            return;
        }

        textBox.Text = factor.Weight == 0
            ? string.Empty
            : FormatWeight(factor.Weight);
        textBox.CaretIndex = textBox.Text.Length;
    }

    private static string FormatWeight(decimal weight)
    {
        return weight.ToString("0.00", RuCulture);
    }
}
