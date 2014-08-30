using System;

using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Infrastructure.Constants;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class StaffgroupViewModel : BaseViewModel, IStaffgroupViewModel, IEventCatcher
    {
        private Repository.DataService.Staffgroup staffgroup;
        public Repository.DataService.Staffgroup Staffgroup
        {
            get { return this.staffgroup; }
            set
            {
                if (value != this.staffgroup)
                {
                    this.staffgroup = value;
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

	    private string foreground = Preferences.StaffForeground;
        public string Foreground
        {
            get { return this.foreground; }
            set
            {
                this.foreground = value;
                OnPropertyChanged(() => Background);
            }
        }

        private string background = Preferences.CompositionBackground;
        public string Background
        {
            get { return this.background; }
            set
            {
                this.background = value;
                OnPropertyChanged(() => Background);
            }
        }

        private double bracketScaleX = 1.0;
        public double BracketScaleX
        {
            get { return this.bracketScaleX; }
            set
            {
                this.bracketScaleX = value;
                OnPropertyChanged(() => BracketScaleX);
            }
        }

        private double bracketScaleY = 2.6;
        public double BracketScaleY
        {
            get { return this.bracketScaleY; }
            set
            {
                this.bracketScaleY = value;
                OnPropertyChanged(() => BracketScaleY);
            }
        }

        private string bracketMargin = "-10,12,0,0";
        public string BracketMargin
        {
            get { return this.bracketMargin; }
            set
            {
                this.bracketMargin = value;
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
