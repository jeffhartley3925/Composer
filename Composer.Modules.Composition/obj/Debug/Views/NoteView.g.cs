﻿#pragma checksum "C:\Projects\Composer\Composer.Modules.Composition\Views\NoteView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "65E219E3B977586B8F3622B332171614"
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
    
    
    public partial class NoteView : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.UserControl NoteUserControl;
        
        internal System.Windows.Controls.Canvas LayoutRoot;
        
        internal System.Windows.Media.TranslateTransform AccidentalTranslate;
        
        internal System.Windows.Media.ScaleTransform AccidentalScale;
        
        internal System.Windows.Media.TranslateTransform NoteTranslate;
        
        internal System.Windows.Media.RotateTransform NoteRotate;
        
        internal System.Windows.Controls.Grid Disposition;
        
        internal System.Windows.Shapes.Rectangle SelectorVisibilityProxy;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Composer.Modules.Composition;component/Views/NoteView.xaml", System.UriKind.Relative));
            this.NoteUserControl = ((System.Windows.Controls.UserControl)(this.FindName("NoteUserControl")));
            this.LayoutRoot = ((System.Windows.Controls.Canvas)(this.FindName("LayoutRoot")));
            this.AccidentalTranslate = ((System.Windows.Media.TranslateTransform)(this.FindName("AccidentalTranslate")));
            this.AccidentalScale = ((System.Windows.Media.ScaleTransform)(this.FindName("AccidentalScale")));
            this.NoteTranslate = ((System.Windows.Media.TranslateTransform)(this.FindName("NoteTranslate")));
            this.NoteRotate = ((System.Windows.Media.RotateTransform)(this.FindName("NoteRotate")));
            this.Disposition = ((System.Windows.Controls.Grid)(this.FindName("Disposition")));
            this.SelectorVisibilityProxy = ((System.Windows.Shapes.Rectangle)(this.FindName("SelectorVisibilityProxy")));
        }
    }
}

