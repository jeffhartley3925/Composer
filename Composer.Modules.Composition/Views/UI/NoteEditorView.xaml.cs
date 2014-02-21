using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Unity;
using System.Reactive.Linq;

namespace Composer.Modules.Composition.Views
{
    public partial class NoteEditorView : INoteEditorView
    {
        public NoteEditorView()
        {
            InitializeComponent();
            ServiceLocator.Current.GetInstance<IEventAggregator>();
            //EnableDrag(LayoutRoot, this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
			if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
			{
			    var container = Unity.Container;
				if (!container.IsRegistered<INoteEditorViewModel>())
				{
					container.RegisterType<INoteEditorViewModel, NoteEditorViewModel>(new ContainerControlledLifetimeManager());
				}
				var viewModel = (NoteEditorViewModel)ServiceLocator.Current.GetInstance<INoteEditorViewModel>() ??
				                (NoteEditorViewModel)container.Resolve<INoteEditorViewModel>();
			    DataContext = viewModel;
			}
        }
        public void EnableDrag(UIElement element, Canvas canvas)
        {
            var mousedown = from evt in Observable.FromEventPattern<MouseButtonEventArgs>(element, "MouseLeftButtonDown") select evt.EventArgs.GetPosition(element);
            var mouseup = Observable.FromEventPattern<MouseButtonEventArgs>(canvas, "MouseLeftButtonUp");
            var mousemove = from evt in Observable.FromEventPattern<MouseEventArgs>(canvas, "MouseMove") select evt.EventArgs.GetPosition(canvas);
            var q = from start in mousedown from end in mousemove.TakeUntil(mouseup) select new { X = end.X - start.X, Y = end.Y - start.Y };
            q.Subscribe(value => { SetLeft(element, value.X); SetTop(element, value.Y); });
        }
    }
}
