﻿#pragma checksum "C:\Projects\Composer\Composer.Modules.Composition\Views\UI\CollaborationNotificationView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "257D7FAAED341BB616760BDE4F01333A"
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
    
    
    public partial class CollaborationNotificationView : System.Windows.Controls.Canvas {
        
        internal System.Windows.Media.Animation.Storyboard FadeInStoryboard;
        
        internal System.Windows.Media.Animation.Storyboard BorderStoryboard;
        
        internal System.Windows.Controls.Border LayoutRoot;
        
        internal System.Windows.Media.RotateTransform rt1;
        
        internal System.Windows.Media.GradientStop gs1;
        
        internal System.Windows.Media.GradientStop gs2;
        
        internal System.Windows.Controls.RowDefinition Header;
        
        internal System.Windows.Controls.RowDefinition Footer;
        
        internal System.Windows.Controls.Button CloseButton;
        
        internal System.Windows.Controls.Button SaveButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Composer.Modules.Composition;component/Views/UI/CollaborationNotificationView.xa" +
                        "ml", System.UriKind.Relative));
            this.FadeInStoryboard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("FadeInStoryboard")));
            this.BorderStoryboard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BorderStoryboard")));
            this.LayoutRoot = ((System.Windows.Controls.Border)(this.FindName("LayoutRoot")));
            this.rt1 = ((System.Windows.Media.RotateTransform)(this.FindName("rt1")));
            this.gs1 = ((System.Windows.Media.GradientStop)(this.FindName("gs1")));
            this.gs2 = ((System.Windows.Media.GradientStop)(this.FindName("gs2")));
            this.Header = ((System.Windows.Controls.RowDefinition)(this.FindName("Header")));
            this.Footer = ((System.Windows.Controls.RowDefinition)(this.FindName("Footer")));
            this.CloseButton = ((System.Windows.Controls.Button)(this.FindName("CloseButton")));
            this.SaveButton = ((System.Windows.Controls.Button)(this.FindName("SaveButton")));
        }
    }
}

