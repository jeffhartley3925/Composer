using System;
using System.Windows;
using System.Windows.Controls;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed partial class CompositionViewModel
    {
        private const int FooterHeight = 27;
        private const int VerticalScrollOffset = 34;
        private const int HorizontalScrollOffset = 412;

        private double _scrollWidth;
        public double ScrollWidth
        {
            get { return _scrollWidth; }
            set
            {
                _scrollWidth = value;
                OnPropertyChanged(() => ScrollWidth);
            }
        }

        private double _scrollHeight;
        public double ScrollHeight
        {
            get { return _scrollHeight; }
            set
            {
                _scrollHeight = value - ((UploadDetailsVisibility == Visibility.Visible) ? FooterHeight + 20 : 40);
                OnPropertyChanged(() => ScrollHeight);
            }
        }

        private string _scrollBackground = Preferences.CompositionScrollBackground;
        public string ScrollBackground
        {
            get { return _scrollBackground; }
            set
            {
                _scrollBackground = value;
                OnPropertyChanged(() => ScrollBackground);
            }
        }

        private ScrollBarVisibility _scrollVisibility;
        public ScrollBarVisibility ScrollVisibility
        {
            get { return _scrollVisibility; }
            set
            {
                _scrollVisibility = value;
                OnPropertyChanged(() => ScrollVisibility);
            }
        }

        private string _background = Preferences.CompositionBackground;
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private int _blurRadius;
        public int BlurRadius
        {
            get { return _blurRadius; }
            set
            {
                _blurRadius = value;
                OnPropertyChanged(() => BlurRadius);
            }
        }

        private double _selectorHeight;
        public double SelectorHeight
        {
            get { return _selectorHeight; }
            set
            {
                _selectorHeight = value;
                OnPropertyChanged(() => SelectorHeight);
            }
        }

        private double _selectorWidth;
        public double SelectorWidth
        {
            get { return _selectorWidth; }
            set
            {
                _selectorWidth = value;
                OnPropertyChanged(() => SelectorWidth);
            }
        }

        private string _selectorMargin;
        public string SelectorMargin
        {
            get { return _selectorMargin; }
            set
            {
                _selectorMargin = value;
                OnPropertyChanged(() => SelectorMargin);
            }
        }

        private double _rectangleX1;
        public double Rectangle_X1
        {
            get { return _rectangleX1; }
            set
            {
                _rectangleX1 = value;
                OnPropertyChanged(() => Rectangle_X1);
            }
        }

        private double _rectangleY1;
        public double Rectangle_Y1
        {
            get { return _rectangleY1; }
            set
            {
                _rectangleY1 = value;
                OnPropertyChanged(() => Rectangle_Y1);
            }
        }

        private double _rectangleX2;
        public double Rectangle_X2
        {
            get { return _rectangleX2; }
            set
            {
                _rectangleX2 = value;
                OnPropertyChanged(() => Rectangle_X2);
            }
        }

        private double _rectangleY2;
        public double Rectangle_Y2
        {
            get { return _rectangleY2; }
            set
            {
                _rectangleY2 = value;
                OnPropertyChanged(() => Rectangle_Y2);
            }
        }

        private string _scrollOffsets;
        public string ScrollOffsets
        {
            get { return _scrollOffsets; }
            set
            {
                _scrollOffsets = value;
                OnPropertyChanged(() => ScrollOffsets);
            }
        }

        private string GetSelectorMargin()
        {
            return string.Format("{0},{1},{2},{3}", Math.Min(Rectangle_X1, Rectangle_X2), Math.Min(Rectangle_Y1, Rectangle_Y2), 0, 0);
        }

        private double GetSelectorHeight()
        {
            return Math.Max(0, Math.Abs(Rectangle_Y1 - Rectangle_Y2) - 7);
        }

        private double GetSelectorWidth()
        {
            return Math.Max(0, Math.Abs(Rectangle_X1 - Rectangle_X2) - 7);
        }

        private void SubscribeUIEvents()
        {
            EA.GetEvent<AreaSelect>().Subscribe(OnAreaSelect);
            EA.GetEvent<UpdateScrollOffset>().Subscribe(OnUpdateScrollOffset);
            EA.GetEvent<ScaleViewportChanged>().Subscribe(OnScaleViewportChanged, true);
            EA.GetEvent<BlurComposition>().Subscribe(OnBlurComposition);
            EA.GetEvent<ResizeViewport>().Subscribe(OnResizeViewPort);
        }

        public void OnUpdateScrollOffset(Tuple<double, double> payload)
        {
            ScrollOffsets = string.Format("{0}, {1}", payload.Item1, payload.Item2);
        }

        public void OnScaleViewportChanged(double newScale)
        {
            ScaleX = newScale;
            ScaleY = newScale;
            ScrollVisibility = (newScale < .72) ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
        }

        public void OnAreaSelect(object obj)
        {
            var p = (Point)obj;
            switch (EditorState.ClickState)
            {
                case _Enum.ClickState.None:
                    Rectangle_X1 = p.X;
                    Rectangle_Y1 = p.Y;
                    EditorState.ClickState = _Enum.ClickState.First;
                    EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                    break;
                case _Enum.ClickState.First:
                    Rectangle_X2 = p.X;
                    Rectangle_Y2 = p.Y;
                    SelectorMargin = GetSelectorMargin();
                    SelectorHeight = GetSelectorHeight();
                    SelectorWidth = GetSelectorWidth();
                    SelectorVisible = Visibility.Visible;
                    if (EditorState.ClickMode == "Click")
                    {
                        EditorState.ClickState = _Enum.ClickState.Second;
                        SelectAllSelectedNotes();
                        SelectorVisible = Visibility.Collapsed;
                        Rectangle_X1 = 0;
                        Rectangle_Y1 = 0;
                        Rectangle_X2 = 0;
                        Rectangle_Y2 = 0;
                        EditorState.ClickState = _Enum.ClickState.None;
                    }
                    break;
                default:
                    EditorState.ClickState = _Enum.ClickState.None;
                    SelectorVisible = Visibility.Collapsed;
                    EA.GetEvent<DeSelectAll>().Publish(string.Empty);
                    break;
            }
        }

        public void OnResizeViewPort(Point coordinate)
        {
            ScrollWidth = coordinate.X - HorizontalScrollOffset;
            ScrollHeight = coordinate.Y - VerticalScrollOffset;
        }

        public void OnBlurComposition(int radius)
        {
            if (!EditorState.IsSaving)
            {
                BlurRadius = radius;
            }
        }
    }
}
