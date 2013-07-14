using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Composer.Infrastructure.Support
{
    public class FontFamily
    {
        public FontFamily(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; } 
    }
    public class FontSize
    {
        public FontSize(string size)
        {
            this.Size = size;
        }   
        public string Size { get; set; }
    }
}
