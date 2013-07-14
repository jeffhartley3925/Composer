using System;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class SpanViewModel : BaseViewModel, ISpanViewModel
    {
        private LocalSpan _span;
        public LocalSpan Span
        {
            get { return _span; }
            set
            {
                _span = value;
                OnPropertyChanged(() => Span);
            }
        }

        public SpanViewModel(string id)
        {
            var span = (from obj in Cache.Spans where obj.Id == Guid.Parse(id) select obj).DefaultIfEmpty(null).Single();
            if (span != null)
            {
                Span = span;
                DefineCommands();
                SubscribeEvents();
            }
        }

        private Visibility _selectorVisibility = Visibility.Collapsed;
        public Visibility SelectorVisibility
        {
            get { return _selectorVisibility; }
            set
            {
                _selectorVisibility = value;
                OnPropertyChanged(() => SelectorVisibility);
            }
        }

        private void OnMouseLeftButtonUpCommand(object o)
        {
        }

        private ICommand _mouseLeftButonUpCommand;
        public ICommand MouseLeftButtonUpCommand
        {
            get { return _mouseLeftButonUpCommand; }
            set
            {
                _mouseLeftButonUpCommand = value;
                OnPropertyChanged(() => MouseLeftButtonUpCommand);
            }
        }

        public void DefineCommands()
        {
            MouseLeftButtonUpCommand = new DelegatedCommand<object>(OnMouseLeftButtonUpCommand);
        }

        public void SubscribeEvents()
        {
            EA.GetEvent<SelectMeasure>().Subscribe(OnSelectMeasure);
            EA.GetEvent<DeSelectMeasure>().Subscribe(OnDeSelectMeasure);
            EA.GetEvent<DeSelectComposition>().Subscribe(OnDeSelectComposition);
        }

        public void OnDeSelectMeasure(Guid id)
        {
            if (Span.Measure_Id == id)
            {
                SelectorVisibility = Visibility.Collapsed;
            }
        }

        public void OnDeSelectComposition(object obj)
        {
            SelectorVisibility = Visibility.Collapsed;
        }

        public void OnSelectMeasure(Guid id)
        {
            if (Span.Measure_Id == id)
            {
                SelectorVisibility = Visibility.Visible;
            }
        }
    }
}