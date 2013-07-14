using System;

namespace Composer.Infrastructure
{
    public class LocalSpan : BaseViewModel
    {
        public LocalSpan()
        {
            Id = Guid.NewGuid();
            Opacity = Preferences.SpanOpacity;
            Foreground = Preferences.SpanForeground;
            Stroke = Preferences.SpanStroke;
        }

        private int _measureIndex;
        public int MeasureIndex
        {
            get { return _measureIndex; }
            set
            {
                if (_measureIndex != value)
                {
                    _measureIndex = value;
                    OnPropertyChanged(() => MeasureIndex);
                }
            }
        }

        private Guid _id;
        public Guid Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(() => Id);
                }
            }
        }

        private string _stroke =Preferences.SpanStroke;
        public string Stroke
        {
            get { return _stroke; }
            set
            {
                if (_stroke != value)
                {
                    _stroke = value;
                    OnPropertyChanged(() => Stroke);
                }
            }
        }

        private string _foreground = Preferences.SpanForeground;
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    OnPropertyChanged(() => Foreground);
                }
            }
        }
        private double _opacity = Preferences.SpanOpacity;
        public double Opacity
        {
            get { return _opacity; }
            set
            {
                if (Math.Abs(_opacity - value) > 0)
                {
                    _opacity = value;
                    OnPropertyChanged(() => Opacity);
                }
            }
        }
        private string _path;
        public string Path 
        { 
            get { return _path; }
            set 
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged(() => Path);
                }
            }
        }
        private double _locationX;
        public double Location_X
        {
            get { return _locationX; }
            set
            {
                if (Math.Abs(_locationX - value) > 0)
                {
                    _locationX = value + 1;
                    OnPropertyChanged(() => Location_X);
                }
            }
        }
        private double _locationY;
        public double Location_Y
        {
            get { return _locationY; }
            set
            {
                if (Math.Abs(_locationY - value) > 0)
                {
                    _locationY = value;
                    OnPropertyChanged(() => Location_Y);
                }
            }
        }
        private int _orientation;
        public int Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    OnPropertyChanged(() => Orientation);
                }
            }
        }

        private Guid _measureId;
        public Guid Measure_Id
        {
            get { return _measureId; }
            set
            {
                if (_measureId != value)
                {
                    _measureId = value;
                    OnPropertyChanged(() => Measure_Id);
                }
            }
        }
    }
}
