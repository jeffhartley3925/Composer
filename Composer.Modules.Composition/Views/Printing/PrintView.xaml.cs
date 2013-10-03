using System.Windows;
using System.Windows.Controls;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.Extensions;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.ViewModels.Helpers;

namespace Composer.Modules.Composition.Views
{
	public partial class PrintView : IPrintView
	{
		private readonly CompositionViewModel _viewModel;
		private static IEventAggregator _ea;

		public PrintView(CompositionViewModel vm)
		{
			InitializeComponent();
			_viewModel = ServiceLocator.Current.GetInstance<CompositionViewModel>();
			_ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
			_ea.GetEvent<ClosePrintPreview>().Subscribe(OnRemovePrint);
		}

		public void OnRemovePrint(object obj)
		{
			CompositionManager.ShowSocialChannels();
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			DataContext = _viewModel;
		}

		private void Print_Click(object sender, RoutedEventArgs e)
		{
			var items = (ItemsControl)LayoutRoot.FindName("printPages");
			var pages = new List<UIElement>();
			foreach (var item in items.GetItemsAndContainers())
			{
				var contentPresenter = (ContentPresenter)item.Value;
				var printPageContainer = FindVisualChild<StackPanel>(contentPresenter);
				pages.Add(printPageContainer);
			}
			pages.Print("Document", HorizontalAlignment.Left, VerticalAlignment.Top, new Thickness(50), false, true, null);
		}

		private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				var child = VisualTreeHelper.GetChild(obj, i);
				if (child is T)
				{
					return (T)child;
				}
				var childOfChild = FindVisualChild<T>(child);
				if (childOfChild != null)
				{
					return childOfChild;
				}
			}
			return null;
		}
	}
}