using System;
using System.Windows;
using System.Linq;
using System.Windows.Input;
using Composer.Infrastructure;
using Composer.Infrastructure.Events;
using Composer.Modules.Composition.Models;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class SpanViewModel : BaseViewModel, ISpanViewModel, IEventCatcher
    {
        private Span _span;
        public Span Span
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
            var span = (from obj in SpanManager.GlobalSpans where obj.Id == Guid.Parse(id) select obj).DefaultIfEmpty(null).Single();
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

        private void OnMouseLeftButtonUpCommand(object obj)
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

        public void OnDeSelectMeasure(Guid id)
        {
            if (IsTargetVM(id))
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
            if (IsTargetVM(id))
            {
                SelectorVisibility = Visibility.Visible;
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

        public bool IsTargetVM(Guid id)
        {
            return Span.Measure_Id == id;
        }
    }
}