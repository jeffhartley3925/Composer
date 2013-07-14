using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Unity;
using System.Reactive.Linq;

namespace Composer.Modules.Composition.Views
{
    public partial class TranspositionView : Canvas, ITranspositionView
    {
        private static IEventAggregator ea;
        public TranspositionView()
        {
            InitializeComponent();
            ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            EnableDrag(LayoutRoot, this);
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            ea.GetEvent<AnimateViewBorder>().Subscribe(OnAnimateViewBorder);
        }

        public void OnAnimateViewBorder(string viewName)
        {
            if (viewName == "Transpose Panel")
            {
                var fadeIn = this.Resources["FadeInStoryboard"] as Storyboard;
                fadeIn.Begin();

                var borderTracer = this.Resources["BorderStoryboard"] as Storyboard;
                borderTracer.Begin();
            }
        }

        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                TranspositionViewModel viewModel;
                IUnityContainer container = Unity.Container;
                if (!container.IsRegistered<ITranspositionViewModel>())
                {
                    container.RegisterType<ITranspositionViewModel, TranspositionViewModel>(new ContainerControlledLifetimeManager());
                }
                viewModel = (TranspositionViewModel)ServiceLocator.Current.GetInstance<ITranspositionViewModel>();
                if (viewModel == null)
                {
                    viewModel = (TranspositionViewModel)container.Resolve<ITranspositionViewModel>();
                }
                this.DataContext = viewModel;
                ea.GetEvent<HideTransposePanel>().Publish(string.Empty);
            }
        }

        public void EnableDrag(UIElement element, Canvas canvas)
        {
            var mousedown = from evt in Observable.FromEventPattern<MouseButtonEventArgs>(element, "MouseLeftButtonDown") select evt.EventArgs.GetPosition(element);
            var mouseup = Observable.FromEventPattern<MouseButtonEventArgs>(canvas, "MouseLeftButtonUp");
            var mousemove = from evt in Observable.FromEventPattern<MouseEventArgs>(canvas, "MouseMove") select evt.EventArgs.GetPosition(canvas);
            var q = from start in mousedown from end in mousemove.TakeUntil(mouseup) select new { X = end.X - start.X, Y = end.Y - start.Y };
            q.Subscribe(value => { Canvas.SetLeft(element, value.X); Canvas.SetTop(element, value.Y); });
        }
    }
}
