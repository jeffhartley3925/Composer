﻿#pragma checksum "C:\Projects\Composer\Composer.Modules.Composition\Views\StaffView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "CF772B5EBFF0429E52E786A351F6C663"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
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
    
    
    public partial class StaffView : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.UserControl StaffUserControl;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel DimensionPanel;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Composer.Modules.Composition;component/Views/StaffView.xaml", System.UriKind.Relative));
            this.StaffUserControl = ((System.Windows.Controls.UserControl)(this.FindName("StaffUserControl")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.DimensionPanel = ((System.Windows.Controls.StackPanel)(this.FindName("DimensionPanel")));
        }
    }
}
