
namespace Composer.Modules.Composition.Views
{
  public interface IMessageBoxService
  {
    GenericMessageBoxResult Show(string message, string caption, GenericMessageBoxButton buttons);
    void Show(string message, string caption);
  }
}