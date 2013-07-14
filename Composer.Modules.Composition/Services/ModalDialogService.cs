using System;
using Composer.Modules.Composition.Views;

namespace Composer.Modules.Composition.Service
{
  public class ModalDialogService : IModalDialogService
  {
    public void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel, Action<TDialogViewModel> onDialogClose) 
    {
      view.DataContext = viewModel;
      if (onDialogClose != null)
      {
        view.Closed += (sender, e) => onDialogClose(viewModel);
      }
      view.Show();            
    }

    public void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel)
    {
      this.ShowDialog(view, viewModel, null);
    }
  }
}