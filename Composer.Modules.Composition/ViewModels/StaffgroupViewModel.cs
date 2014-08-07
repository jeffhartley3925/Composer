using System;
using System.Linq;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class StaffgroupViewModel : BaseViewModel, IStaffgroupViewModel, IEventCatcher
    {
        private Repository.DataService.Staffgroup _staffgroup;
        public Repository.DataService.Staffgroup Staffgroup
        {
            get { return _staffgroup; }
            set
            {
                if (value != _staffgroup)
                {
                    _staffgroup = value;
                    OnPropertyChanged(() => Staffgroup);
                }
            }
        }

        public StaffgroupViewModel(string id)
        {
            Staffgroup = Utils.GetStaffgroup(Guid.Parse(id));
            DefineCommands();
            SubscribeEvents();
        }

        public override void OnClick(object obj)
        {
            Guid guid;
            if (Guid.TryParse(obj.ToString(), out guid))
            {
                if (guid == Staffgroup.Id)
                {
                }
            }
        }

        private bool IsStaffGroupEmpty()
        {
            return Staffgroup.Staffs.SelectMany(staff => staff.Measures).All(measure => !(Convert.ToDouble((from c in measure.Chords select c.Duration).Sum()) > 0));
        }

        private string _foreground = Preferences.StaffForeground;
        public string Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                OnPropertyChanged(() => Background);
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

        private double _bracketScaleX = 1.0;
        public double BracketScaleX
        {
            get { return _bracketScaleX; }
            set
            {
                _bracketScaleX = value;
                OnPropertyChanged(() => BracketScaleX);
            }
        }

        private double _bracketScaleY = 2.6;
        public double BracketScaleY
        {
            get { return _bracketScaleY; }
            set
            {
                _bracketScaleY = value;
                OnPropertyChanged(() => BracketScaleY);
            }
        }

        private string _bracketMargin = "-10,12,0,0";
        public string BracketMargin
        {
            get { return _bracketMargin; }
            set
            {
                _bracketMargin = value;
                OnPropertyChanged(() => BracketMargin);
            }
        }

        public void DefineCommands()
        {
            ClickCommand = new DelegatedCommand<object>(OnClick);
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<StaffClicked>().Subscribe(OnClick);
            EA.GetEvent<SelectStaffgroup>().Subscribe(OnSelectStaffgroup);
            EA.GetEvent<AdjustBracketHeight>().Subscribe(OnAdjustBracketHeight);
        }

        public void OnAdjustBracketHeight(object obj)
        {
            var a = (Defaults.BracketHeightBaseline + (EditorState.VerseCount * Defaults.VerseHeight)) / Defaults.BracketHeightBaseline;
            BracketScaleY = a * Defaults.BracketScaleYBaseline;
            BracketMargin = string.Format("{0},{1},{2},{3}",-10, Math.Max((12 - (EditorState.VerseCount * 3)),0),0,0);
        }

        public void OnSelectStaffgroup(Guid id)
        {
            if (Staffgroup != null && Staffgroup.Id == id)
            {
                foreach (var staff in Staffgroup.Staffs)
                {
                    EA.GetEvent<SelectStaff>().Publish(staff.Id);
                }
            }
        }

        public bool IsTargetVM(Guid Id)
        {
            throw new NotImplementedException();
        }
    }
}
