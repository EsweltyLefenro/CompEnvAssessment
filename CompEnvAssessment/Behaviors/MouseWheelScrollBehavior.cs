using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CompEnvAssessment.Behaviors;

public static class MouseWheelScrollBehavior
{
    public static readonly DependencyProperty ForwardToParentAtEdgeProperty =
        DependencyProperty.RegisterAttached(
            "ForwardToParentAtEdge",
            typeof(bool),
            typeof(MouseWheelScrollBehavior),
            new PropertyMetadata(false, OnForwardToParentAtEdgeChanged));

    public static readonly DependencyProperty ForwardToParentOnlyWhenNotScrollableProperty =
        DependencyProperty.RegisterAttached(
            "ForwardToParentOnlyWhenNotScrollable",
            typeof(bool),
            typeof(MouseWheelScrollBehavior),
            new PropertyMetadata(false, OnForwardToParentAtEdgeChanged));

    public static bool GetForwardToParentAtEdge(DependencyObject element)
    {
        return (bool)element.GetValue(ForwardToParentAtEdgeProperty);
    }

    public static void SetForwardToParentAtEdge(DependencyObject element, bool value)
    {
        element.SetValue(ForwardToParentAtEdgeProperty, value);
    }

    public static bool GetForwardToParentOnlyWhenNotScrollable(DependencyObject element)
    {
        return (bool)element.GetValue(ForwardToParentOnlyWhenNotScrollableProperty);
    }

    public static void SetForwardToParentOnlyWhenNotScrollable(DependencyObject element, bool value)
    {
        element.SetValue(ForwardToParentOnlyWhenNotScrollableProperty, value);
    }

    private static void OnForwardToParentAtEdgeChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is not UIElement element)
        {
            return;
        }

        element.PreviewMouseWheel -= OnPreviewMouseWheel;
        if (GetForwardToParentAtEdge(dependencyObject)
            || GetForwardToParentOnlyWhenNotScrollable(dependencyObject))
        {
            element.PreviewMouseWheel += OnPreviewMouseWheel;
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
    {
        if (sender is not DependencyObject source)
        {
            return;
        }

        ScrollViewer? localScrollViewer = source as ScrollViewer ?? FindVisualChild<ScrollViewer>(source);
        if (GetForwardToParentOnlyWhenNotScrollable(source)
            && localScrollViewer is not null
            && HasScrollableVerticalContent(localScrollViewer))
        {
            args.Handled = true;
            localScrollViewer.ScrollToVerticalOffset(localScrollViewer.VerticalOffset - args.Delta);
            return;
        }

        if (localScrollViewer is not null && CanScrollVertically(localScrollViewer, args.Delta))
        {
            return;
        }

        ScrollViewer? parentScrollViewer = FindParentScrollViewer(source, localScrollViewer);
        if (parentScrollViewer is null)
        {
            return;
        }

        args.Handled = true;
        parentScrollViewer.ScrollToVerticalOffset(parentScrollViewer.VerticalOffset - args.Delta);
    }

    private static bool HasScrollableVerticalContent(ScrollViewer scrollViewer)
    {
        const double tolerance = 0.1;
        return scrollViewer.ScrollableHeight > tolerance;
    }

    private static bool CanScrollVertically(ScrollViewer scrollViewer, int delta)
    {
        const double tolerance = 0.1;

        if (scrollViewer.ScrollableHeight <= tolerance)
        {
            return false;
        }

        return delta > 0
            ? scrollViewer.VerticalOffset > tolerance
            : scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight - tolerance;
    }

    private static ScrollViewer? FindParentScrollViewer(
        DependencyObject element,
        ScrollViewer? localScrollViewer)
    {
        DependencyObject? parent = GetParent(element);

        while (parent is not null)
        {
            if (parent is ScrollViewer scrollViewer && !ReferenceEquals(scrollViewer, localScrollViewer))
            {
                return scrollViewer;
            }

            parent = GetParent(parent);
        }

        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        if (parent is not Visual and not Visual3D)
        {
            return null;
        }

        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int index = 0; index < childCount; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild)
            {
                return typedChild;
            }

            T? descendant = FindVisualChild<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static DependencyObject? GetParent(DependencyObject element)
    {
        if (element is Visual or Visual3D)
        {
            return VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element);
        }

        return LogicalTreeHelper.GetParent(element);
    }
}
