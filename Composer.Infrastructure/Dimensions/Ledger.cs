using System;
namespace Composer.Infrastructure
{
    public class Ledger : BaseViewModel
    {
        public Ledger()
        {
            Id = Guid.NewGuid();
            Opacity = Preferences.LedgerOpacity;
            Stroke = Preferences.LedgerStroke;
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

        private string _stroke = Preferences.LedgerStroke;
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

        private int _size;
        public int Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged(() => Size);
                }
            }
        }

        private double _opacity = Preferences.LedgerOpacity;
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

        private double _locationX;
        public double Location_X
        {
            get { return _locationX; }
            set
            {
                if (Math.Abs(_locationX - value) > 0)
                {
                    _locationX = value;
                    OnPropertyChanged(() => Location_X);
                }
            }
        }

        private Guid _noteId;
        public Guid Note_Id
        {
            get { return _noteId; }
            set
            {
                if (_noteId != value)
                {
                    _noteId = value;
                    OnPropertyChanged(() => Note_Id);
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
    }
}
