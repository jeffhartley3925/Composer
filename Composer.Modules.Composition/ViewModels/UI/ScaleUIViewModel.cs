using Composer.Infrastructure;
using Microsoft.Practices.Unity;
using Composer.Infrastructure.Events;

namespace Composer.Modules.Composition.ViewModels
{
    public sealed class UIScaleViewModel : BaseViewModel, IUIScaleViewModel
    {
        public UIScaleViewModel(IUnityContainer container)
            : base()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            EA.GetEvent<ToggleUIScaleEnable>().Subscribe(OnToggleUiScaleEnable);
        }

        public void OnToggleUiScaleEnable(bool state)
        {
            IsEnabled = state;
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(() => IsEnabled);
            }
        }
    }
}