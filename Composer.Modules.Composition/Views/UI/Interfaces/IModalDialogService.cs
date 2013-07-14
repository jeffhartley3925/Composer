using System;

namespace Composer.Modules.Composition.Views
{
  public interface IModalDialogService
  {
    void ShowDialog<TViewModel>(IModalWindow view, TViewModel viewModel, Action<TViewModel> onDialogClose);

    void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel);
  }
}