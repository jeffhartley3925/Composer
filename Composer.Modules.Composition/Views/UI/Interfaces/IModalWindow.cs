using System;

namespace Composer.Modules.Composition.Views
{
    public interface IModalWindow
    {
        bool? DialogResult { get; set; }  
        event EventHandler Closed;  
        void Show();  
        object DataContext { get; set; }  
        void Close();  
    }
}
