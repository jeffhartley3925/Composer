using System;
using System.Windows;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.ServiceLocation;

namespace Composer.Modules.Palettes.ViewModels
{
    public class BasePaletteViewModel : INotifyPropertyChanged
    {
        protected IEventAggregator Ea { get; set; }

        public BasePaletteViewModel()
        {
            Ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            Hide(new object());
        }

        private string _margin = Infrastructure.Constants.Palette.DefaultPaletteMargin;
        public string Margin
        {
            get { return _margin; }
            set
            {
                if (_margin != value)
                {
                    _margin = value;
                    OnPropertyChanged(() => Margin);
                }
            }
        }
        private Double _scaleX = 1.0;
        public Double ScaleX
        {
            get { return _scaleX; }
            set
            {
                if (Math.Abs(_scaleX - value) > 0)
                {
                    _scaleX = value;
                    OnPropertyChanged(() => ScaleX);
                }
            }
        }
        private Double _scaleY = 1.0;
        public Double ScaleY
        {
            get { return _scaleY; }
            set
            {
                if (Math.Abs(_scaleY - value) > 0)
                {
                    _scaleY = value;
                    OnPropertyChanged(() => ScaleY);
                }
            }
        }
        private GridLength _paletteWidth = new GridLength(Infrastructure.Constants.Palette.DefaultPaletteWidth);
        public GridLength PaletteWidth
        {
            get { return _paletteWidth; }
            set
            {
                _paletteWidth = value;
                OnPropertyChanged(() => PaletteWidth);
            }
        }
        private GridLength _titleBarWidth = new GridLength(Infrastructure.Constants.Palette.DefaultPaletteWidth - 2);
        public GridLength TitleBarWidth
        {
            get { return _titleBarWidth; }
            set
            {
                _titleBarWidth = value;
                OnPropertyChanged(() => TitleBarWidth);
            }
        }
        private Visibility _titleBarVisible = Visibility.Collapsed;
        public Visibility TitleBarVisible
        {
            get { return _titleBarVisible; }
            set
            {
                _titleBarVisible = value;
                OnPropertyChanged(() => TitleBarVisible);
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
        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                if (value != _caption)
                {
                    _caption = value;
                    OnPropertyChanged(() => Caption);
                }
            }
        }
        private string _id;
        public string Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged(() => Id);
                }
            }
        }
        private List<PaletteItem> _items;
        public List<PaletteItem> Items
        {
            get { return _items; }
            set
            {
                if (value != _items)
                {
                    _items = value;
                    OnPropertyChanged(() => Items);
                }
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
                if (memberExpr != null)
                {
                    string propertyName = memberExpr.Member.Name;
                    OnPropertyChanged(propertyName);
                }
            }
        }
    }
}
