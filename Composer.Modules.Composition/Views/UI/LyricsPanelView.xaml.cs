using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Composer.Modules.Composition.ViewModels;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Infrastructure.Events;
using Microsoft.Practices.Unity;
using System.Reactive.Linq;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Views
{
    public partial class LyricsPanelView : ILyricsPanelView
    {
        private static IEventAggregator _ea;
        public LyricsPanelView()
        {
            InitializeComponent();
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            EnableDrag(LayoutRoot, this);
            SubscribeEvents();
        }

        public void SubscribeEvents()
        {
            _ea.GetEvent<AnimateViewBorder>().Subscribe(OnAnimateViewBorder);
        }

        public void OnAnimateViewBorder(string viewName)
        {
            if (viewName != "Lyrics Panel") return;
            var fadeIn = Resources["FadeInStoryboard"] as Storyboard;
            var borderTracer = Resources["BorderStoryboard"] as Storyboard;
            if (fadeIn != null && borderTracer != null)
            {
                fadeIn.Begin();
                borderTracer.Begin();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                IUnityContainer container = Unity.Container;
                if (!container.IsRegistered<ILyricsPanelViewModel>())
                {
                    container.RegisterType<ILyricsPanelViewModel, LyricsPanelViewModel>(new ContainerControlledLifetimeManager());
                }
                var viewModel = (LyricsPanelViewModel)ServiceLocator.Current.GetInstance<ILyricsPanelViewModel>() ??
                                (LyricsPanelViewModel)container.Resolve<ILyricsPanelViewModel>();
                DataContext = viewModel;
                _ea.GetEvent<HideLyricsPanel>().Publish(string.Empty);
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

        private void Editor_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<ReorderVerses>().Publish(new Tuple<_Enum.Direction, int>(_Enum.Direction.Up, Constants.INVALID_VERSE_INDEX));
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<ReorderVerses>().Publish(new Tuple<_Enum.Direction, int>(_Enum.Direction.Down, Constants.INVALID_VERSE_INDEX));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<DeleteVerse>().Publish(Constants.INVALID_VERSE_INDEX);
        }

        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<ToggleVerseInclusion>().Publish(new Tuple<string, int>("", Constants.INVALID_VERSE_INDEX));
        }

        private void CloneClick(object sender, RoutedEventArgs e)
        {
            _ea.GetEvent<CloneVerse>().Publish(string.Empty);
        }
    }
}
