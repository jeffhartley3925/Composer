﻿#pragma checksum "C:\Projects\Composer\Composer.Modules.Composition\Views\CompositionView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "FD81C5AB77140B6EC6F8A64FBEC060E7"
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
    
    
    public partial class CompositionView : System.Windows.Controls.Canvas {
        
        internal System.Windows.Controls.Border LayoutRoot;
        
        internal System.Windows.Controls.Image Reflection;
        
        internal System.Windows.Controls.ScrollViewer Scroller;
        
        internal System.Windows.Controls.Grid RootGrid;
        
        internal System.Windows.Controls.StackPanel pnlUploadDetails;
        
        internal System.Windows.Controls.TextBlock txtResponse;
        
        internal System.Windows.Controls.TextBlock txtRawSize;
        
        internal System.Windows.Controls.TextBlock txtCompressedSize;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Composer.Modules.Composition;component/Views/CompositionView.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Border)(this.FindName("LayoutRoot")));
            this.Reflection = ((System.Windows.Controls.Image)(this.FindName("Reflection")));
            this.Scroller = ((System.Windows.Controls.ScrollViewer)(this.FindName("Scroller")));
            this.RootGrid = ((System.Windows.Controls.Grid)(this.FindName("RootGrid")));
            this.pnlUploadDetails = ((System.Windows.Controls.StackPanel)(this.FindName("pnlUploadDetails")));
            this.txtResponse = ((System.Windows.Controls.TextBlock)(this.FindName("txtResponse")));
            this.txtRawSize = ((System.Windows.Controls.TextBlock)(this.FindName("txtRawSize")));
            this.txtCompressedSize = ((System.Windows.Controls.TextBlock)(this.FindName("txtCompressedSize")));
        }
    }
}

