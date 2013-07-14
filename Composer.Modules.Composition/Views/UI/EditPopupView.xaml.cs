using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using Microsoft.Practices.Unity;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Views
{
    public partial class EditPopupView : IEditPopupView
    {
        private static IEventAggregator _ea;
        public EditPopupView()
        {
            InitializeComponent();
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
        }

        private void PopupMenu_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
			{
			    IUnityContainer container = Unity.Container;
				if (!container.IsRegistered<IEditPopupViewModel>())
				{
					container.RegisterType<IEditPopupViewModel, EditPopupViewModel>(new ContainerControlledLifetimeManager());
				}
				EditPopupViewModel viewModel = (EditPopupViewModel)ServiceLocator.Current.GetInstance<IEditPopupViewModel>() ??
				                               (EditPopupViewModel)container.Resolve<IEditPopupViewModel>();
			    DataContext = viewModel;
                _ea.GetEvent<HideEditPopup>().Publish(string.Empty);
			}
        }

		private void PopupMenu_ItemClick(object sender, DevExpress.AgMenu.AgMenuEventEventArgs e)
		{
		    _ea.GetEvent<HideEditPopup>().Publish(string.Empty);
		}

		private void AgMenuItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            try
            {
                var item = (DevExpress.AgMenu.AgMenuItem)sender;
                var category = (item.Tag==null) ? "" : item.Tag.ToString();
                _ea.GetEvent<EditPopupItemClicked>().Publish(new Tuple<string, string, _Enum.PasteCommandSource>(item.Header.ToString(), category, _Enum.PasteCommandSource.User));
            }
            catch(Exception)
            {
                var barMenuItem = (Grid)sender;
                var barId = short.Parse(barMenuItem.Tag.ToString());
                _ea.GetEvent<UpdateMeasureBar>().Publish(barId);
            }
        }
    }
}
