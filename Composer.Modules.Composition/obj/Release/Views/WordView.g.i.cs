﻿#pragma checksum "C:\Projects\Composer\Composer.Modules.Composition\Views\WordView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "391534ED5D9636C9BA185050F30CDE7A"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Composer.Modules.Composition.Views {
    
    
    public partial class WordView : System.Windows.Controls.Canvas {
        
        internal System.Windows.Controls.Canvas WordUserControl;
        
        internal System.Windows.Controls.Canvas LayoutRoot;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/Composer.Modules.Composition;component/Views/WordView.xaml", System.UriKind.Relative));
            this.WordUserControl = ((System.Windows.Controls.Canvas)(this.FindName("WordUserControl")));
            this.LayoutRoot = ((System.Windows.Controls.Canvas)(this.FindName("LayoutRoot")));
        }
    }
}
