using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Threading;
using Composer.Infrastructure.Behavior;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Infrastructure
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected IEventAggregator EA { get; set; }

        private DelegatedCommand<object> _clickCommand;
        public DelegatedCommand<object> ClickCommand
        {
            get { return _clickCommand; }
            set
            {
                _clickCommand = value;
                OnPropertyChanged(() => ClickCommand);
            }
        }

        public virtual bool CanHandleMouseEnter(object obj)
        {
            return true;
        }
        public virtual bool CanHandleMouseLeave(object obj)
        {
            return true;
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseEnterCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseEnterCommand
        {
            get{ return _mouseEnterCommand; }
            set
            {
                if (value != _mouseEnterCommand)
                {
                    _mouseEnterCommand = value;
                    OnPropertyChanged(() => MouseEnterCommand);
                }
            }
        }

        private ExtendedDelegateCommand<ExtendedCommandParameter> _mouseLeaveCommand;
        public ExtendedDelegateCommand<ExtendedCommandParameter> MouseLeaveCommand
        {
            get
            {
                return _mouseLeaveCommand;
            }
            set
            {
                if (value != _mouseLeaveCommand)
                {
                    _mouseLeaveCommand = value;
                    OnPropertyChanged(() => MouseLeaveCommand);
                }
            }
        }

        private string _emptyBind = string.Empty;
        public string EmptyBind
        {
            get
            {
                return _emptyBind;
            }
            set
            {
                if (value != _emptyBind)
                {
                    _emptyBind = value;
                    OnPropertyChanged(() => EmptyBind);
                }
            }
        }

        public virtual void OnChildClick(object obj) { }

        public virtual void OnClick(object obj) { }

        public virtual void OnMouseMove(ExtendedCommandParameter param) { }

        public virtual void OnMouseLeave(ExtendedCommandParameter param) { }

        public virtual void OnMouseEnter(ExtendedCommandParameter param) { }

        protected BaseViewModel()
        {
            Dispatcher = Deployment.Current.Dispatcher;
            EA = ServiceLocator.Current.GetInstance<IEventAggregator>();
            Debugging = true;
            HideSelector();
        }

        private Double _scaleX;
        public Double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                OnPropertyChanged(() => ScaleX);
            }
        }

        private Double _scaleY;
        public Double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                OnPropertyChanged(() => ScaleY);
            }
        }

        private Visibility _visible;
        public Visibility Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                OnPropertyChanged(() => Visible);
            }
        }
        
        public void Hide(object obj)
        {
            Visible = Visibility.Collapsed;
        }

        public void Show(object obj)
        {
            Visible = Visibility.Visible;
        }
        
        public void Hide()
        {
            Visible = Visibility.Collapsed;
        }

        public bool Debugging { get; set; }

        public virtual bool HideSelector()
        {
            SelectorVisible = Visibility.Collapsed;
            IsSelected = false;
            return true;
        }

        public virtual bool ShowSelector()
        {
            SelectorVisible = Visibility.Visible;
            IsSelected = true;
            return true;
        }

        private int _selectorX;
        public int Selector_X
        {
            get { return _selectorX; }
            set
            {
                if (value != _selectorX)
                {
                    _selectorX = value;
                    OnPropertyChanged(() => Selector_X);
                }
            }
        }

        public bool IsSelected { get; set; }

        private Visibility _selectorVisible = Visibility.Collapsed;

        public Visibility SelectorVisible
        {
            get { return _selectorVisible; }
            set
            {
                _selectorVisible = value;
                OnPropertyChanged(() => SelectorVisible);
            }
        }

        protected Dispatcher Dispatcher { get; private set; }

        protected void InvokeOnUIThread(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }
                                                                                                                                                          
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = propertyExpression.Body as MemberExpression;
                if (memberExpr == null) throw new ArgumentNullException("memberExpr");
                var propertyName = memberExpr.Member.Name;
                OnPropertyChanged(propertyName);
            }
        }

        public void HideVisualElements()
        {
            HideSelector();
        }
    }
}