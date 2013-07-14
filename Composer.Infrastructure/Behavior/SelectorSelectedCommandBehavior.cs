using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows.Controls.Primitives;

namespace Composer.Infrastructure.Behavior
{
    public class SelectedCommandBehavior : CommandBehaviorBase<Selector>
    {
        public SelectedCommandBehavior(Selector selectableObject)
            : base(selectableObject)
        {
            selectableObject.SelectionChanged += OnSelectionChanged;
        }

        void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommandParameter = TargetObject.SelectedItem;
            ExecuteCommand();
        }
    }
    public static class Selected
    {
        private static readonly DependencyProperty SelectedCommandBehaviorProperty = DependencyProperty.RegisterAttached(
            "SelectedCommandBehavior",
            typeof(SelectedCommandBehavior),
            typeof(Selected),
            null);

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(Selected),
            new PropertyMetadata(OnSetCommandCallback));

        public static void SetCommand(Selector selector, ICommand command)
        {
            selector.SetValue(CommandProperty, command);
        }
        public static ICommand GetCommand(Selector selector)
        {
            return selector.GetValue(CommandProperty) as ICommand;
        }
        private static void OnSetCommandCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var selector = dependencyObject as Selector;
            if (selector != null)
            {
                GetOrCreateBehavior(selector).Command = e.NewValue as ICommand;
            }
        }
        private static SelectedCommandBehavior GetOrCreateBehavior(Selector selector)
        {
            var behavior = selector.GetValue(SelectedCommandBehaviorProperty) as SelectedCommandBehavior;
            if (behavior == null)
            {
                behavior = new SelectedCommandBehavior(selector);
                selector.SetValue(SelectedCommandBehaviorProperty, behavior);
            }
            return behavior;
        }
    }
}
