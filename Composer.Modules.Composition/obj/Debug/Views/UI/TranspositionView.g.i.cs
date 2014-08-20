﻿#pragma checksum "C:\Projects\Composer\Composer.Modules.Composition\Views\UI\TranspositionView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7E6FB2D38A554A3A15F070C0B818B690"
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
    
    
    public partial class TranspositionView : System.Windows.Controls.Canvas {
        
        internal System.Windows.Media.Animation.Storyboard FadeInStoryboard;
        
        internal System.Windows.Media.Animation.Storyboard BorderStoryboard;
        
        internal System.Windows.Controls.Border LayoutRoot;
        
        internal System.Windows.Media.RotateTransform rt1;
        
        internal System.Windows.Media.GradientStop gs1;
        
        internal System.Windows.Media.GradientStop gs2;
        
        internal System.Windows.Controls.RowDefinition Header;
        
        internal System.Windows.Controls.RowDefinition Footer;
        
        internal System.Windows.Controls.CheckBox EnableOctaveTransposition;
        
        internal System.Windows.Controls.CheckBox EnableKeyTransposition;
        
        internal System.Windows.Controls.CheckBox EnableIntervalTransposition;
        
        internal System.Windows.Controls.RadioButton OctaveUpRadioButton;
        
        internal System.Windows.Controls.RadioButton OctaveDownRadioButton;
        
        internal System.Windows.Controls.ComboBox keyComboBox;
        
        internal System.Windows.Controls.RadioButton IntervalUpRadioButton;
        
        internal System.Windows.Controls.RadioButton IntervalDownRadioButton;
        
        internal System.Windows.Controls.ComboBox intervalComboBox;
        
        internal System.Windows.Controls.Button TransposeButton;
        
        internal System.Windows.Controls.Button CancelButton;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/Composer.Modules.Composition;component/Views/UI/TranspositionView.xaml", System.UriKind.Relative));
            this.FadeInStoryboard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("FadeInStoryboard")));
            this.BorderStoryboard = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BorderStoryboard")));
            this.LayoutRoot = ((System.Windows.Controls.Border)(this.FindName("LayoutRoot")));
            this.rt1 = ((System.Windows.Media.RotateTransform)(this.FindName("rt1")));
            this.gs1 = ((System.Windows.Media.GradientStop)(this.FindName("gs1")));
            this.gs2 = ((System.Windows.Media.GradientStop)(this.FindName("gs2")));
            this.Header = ((System.Windows.Controls.RowDefinition)(this.FindName("Header")));
            this.Footer = ((System.Windows.Controls.RowDefinition)(this.FindName("Footer")));
            this.EnableOctaveTransposition = ((System.Windows.Controls.CheckBox)(this.FindName("EnableOctaveTransposition")));
            this.EnableKeyTransposition = ((System.Windows.Controls.CheckBox)(this.FindName("EnableKeyTransposition")));
            this.EnableIntervalTransposition = ((System.Windows.Controls.CheckBox)(this.FindName("EnableIntervalTransposition")));
            this.OctaveUpRadioButton = ((System.Windows.Controls.RadioButton)(this.FindName("OctaveUpRadioButton")));
            this.OctaveDownRadioButton = ((System.Windows.Controls.RadioButton)(this.FindName("OctaveDownRadioButton")));
            this.keyComboBox = ((System.Windows.Controls.ComboBox)(this.FindName("keyComboBox")));
            this.IntervalUpRadioButton = ((System.Windows.Controls.RadioButton)(this.FindName("IntervalUpRadioButton")));
            this.IntervalDownRadioButton = ((System.Windows.Controls.RadioButton)(this.FindName("IntervalDownRadioButton")));
            this.intervalComboBox = ((System.Windows.Controls.ComboBox)(this.FindName("intervalComboBox")));
            this.TransposeButton = ((System.Windows.Controls.Button)(this.FindName("TransposeButton")));
            this.CancelButton = ((System.Windows.Controls.Button)(this.FindName("CancelButton")));
        }
    }
}

