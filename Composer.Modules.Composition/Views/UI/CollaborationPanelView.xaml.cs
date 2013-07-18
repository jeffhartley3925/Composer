using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Composer.Modules.Composition.ViewModels;
using System.Reactive.Linq;

namespace Composer.Modules.Composition.Views
{
    public partial class CollaborationPanelView : Canvas, ICollaborationPanelView
	{
		public CollaborationPanelView()
		{
			InitializeComponent();
			Loaded += CollaborationPanelView_Loaded;
			EnableDrag(LayoutRoot, this);
		}

		void CollaborationPanelView_Loaded(object sender, RoutedEventArgs e)
		{
            var fadeIn = Resources["FadeInStoryboard"] as Storyboard;
		    if (fadeIn != null) fadeIn.Begin();

		    var borderTracer = Resources["BorderStoryboard"] as Storyboard;
		    if (borderTracer != null) borderTracer.Begin();

		    if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
			{
				DataContext = new CollaborationPanelViewModel();
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
