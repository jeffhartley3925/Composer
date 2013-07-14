using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Printing;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Composer.Infrastructure.Events;
using Microsoft.Practices.ServiceLocation;
using Composer.Modules.Composition.ViewModels.Helpers;
using Composer.Infrastructure;

namespace Composer.Modules.Composition.Extensions
{
    public static class PrintExtensions
    {
        private static IEventAggregator _ea;

        public static void Print(this FrameworkElement element, string document, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, Thickness pageMargin, bool printLandscape, bool shrinkToFit, Action onPrintComplete)
        {
            Print(new List<FrameworkElement>() { element }, document, horizontalAlignment, verticalAlignment, pageMargin, printLandscape, shrinkToFit, onPrintComplete);
        }

        public static void Print(this UIElementCollection elements, string document, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, Thickness pageMargin, bool printLandscape, bool shrinkToFit, Action onPrintComplete)
        {
            Print(elements.ToList(), document, horizontalAlignment, verticalAlignment, pageMargin, printLandscape, shrinkToFit, onPrintComplete);
        }

        private static void printDocument_BeginPrint(object sender, BeginPrintEventArgs e)
        {
            _ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _ea.GetEvent<ShowBusyIndicator>().Publish(string.Empty);
        }

        private static void printDocument_EndPrint(object sender, EndPrintEventArgs e)
        {
            _ea.GetEvent<ClosePrintPreview>().Publish(string.Empty);
            _ea.GetEvent<HideBusyIndicator>().Publish(string.Empty);
            CompositionManager.ShowSocialChannels();
        }

        public static void Print<T>(this List<T> elements, string document, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, Thickness pageMargin, bool printLandscape, bool shrinkToFit, Action onPrintComplete)
        {
            var printDocument = new PrintDocument();
            document = (string.IsNullOrEmpty(document)) ? "Print Document" : document;
            int currentItemIndex = 0;


            printDocument.BeginPrint += printDocument_BeginPrint;
            printDocument.EndPrint += printDocument_EndPrint;

            printDocument.PrintPage += delegate(object sender, PrintPageEventArgs evt)
            {
                if (!typeof(FrameworkElement).IsAssignableFrom(elements[currentItemIndex].GetType()))
                {
                    throw new Exception("Element must be an " +
                          "object inheriting from FrameworkElement");
                }

                var element = elements[currentItemIndex] as StackPanel;
                if (element != null)
                {
                    if (element.Parent == null || element.ActualWidth == double.NaN || element.ActualHeight == double.NaN)
                    {
                        throw new Exception("Element must be rendered, and must have a parent in order to print.");
                    }
                }
                else
                {
                    throw new Exception("Element must not be null in order to print.");
                }

                var transformGroup = new TransformGroup();

                //First move to middle of page...
                transformGroup.Children.Add(new TranslateTransform()
                {
                    X = (evt.PrintableArea.Width - element.ActualWidth) / 2,
                    Y = (evt.PrintableArea.Height - element.ActualHeight) / 2
                });
                double scale = 1;
                if (printLandscape)
                {
                    //Then, rotate around the center
                    transformGroup.Children.Add(new RotateTransform()
                    {
                        Angle = 90,
                        CenterX = evt.PrintableArea.Width / 2,
                        CenterY = evt.PrintableArea.Height / 2
                    });

                    if (shrinkToFit)
                    {
                        if ((element.ActualWidth + pageMargin.Left +
                              pageMargin.Right) > evt.PrintableArea.Height)
                        {
                            scale = Math.Round(evt.PrintableArea.Height /
                              (element.ActualWidth + pageMargin.Left + pageMargin.Right), 2);
                        }
                        if ((element.ActualHeight + pageMargin.Top + pageMargin.Bottom) >
                                                    evt.PrintableArea.Width)
                        {
                            double scale2 = Math.Round(evt.PrintableArea.Width /
                              (element.ActualHeight + pageMargin.Top + pageMargin.Bottom), 2);
                            scale = (scale2 < scale) ? scale2 : scale;
                        }
                    }
                }
                else if (shrinkToFit)
                {
                    //Scale down to fit the page + margin

                    if ((EditorState.GlobalStaffWidth + pageMargin.Left +
                            pageMargin.Right) > evt.PrintableArea.Width)
                    {
                        scale = Math.Round(evt.PrintableArea.Width /
                          (EditorState.GlobalStaffWidth + pageMargin.Left + pageMargin.Right), 2);
                    }
                    if ((element.ActualHeight + pageMargin.Top + pageMargin.Bottom) >
                                 evt.PrintableArea.Height)
                    {
                        //TODOL HARD CODED VALUE - 1000. Should be actualHeight of the element containing the printed page
                        double scale2 = Math.Round(evt.PrintableArea.Height / (1000 + pageMargin.Top + pageMargin.Bottom), 2);
                        scale = (scale2 < scale) ? scale2 : scale;
                    }
                }

                //Scale down to fit the page + margin
                if (scale != 1)
                {
                    transformGroup.Children.Add(new ScaleTransform()
                    {
                        ScaleX = scale,
                        ScaleY = scale,
                        CenterX = evt.PrintableArea.Width / 2,
                        CenterY = evt.PrintableArea.Height / 2
                    });
                }

                if (printLandscape)
                {
                    transformGroup.Children.Add(new TranslateTransform()
                    {
                        X = 0,
                        Y = pageMargin.Top - (evt.PrintableArea.Height -
                            (element.ActualWidth * scale)) / 2
                    });
                }
                else
                {
                    transformGroup.Children.Add(new TranslateTransform()
                    {
                        X = 0,
                        Y = pageMargin.Top - (evt.PrintableArea.Height -
                        (element.ActualHeight * scale)) / 2
                    });
                }
                evt.PageVisual = element;
                evt.PageVisual.RenderTransform = transformGroup;

                //Increment to next item,
                currentItemIndex++;

                //If the currentItemIndex is less than the number of elements, keep printing
                evt.HasMorePages = currentItemIndex < elements.Count;
            };

            printDocument.EndPrint += delegate
                                          {
                foreach (var item in elements)
                {
                    var element = item as FrameworkElement;
                    //Reset everything...
                    var transformGroup = new TransformGroup();
                    transformGroup.Children.Add(new ScaleTransform() { ScaleX = 1, ScaleY = 1 });
                    transformGroup.Children.Add(new RotateTransform() { Angle = 0 });
                    transformGroup.Children.Add(new TranslateTransform() { X = 0, Y = 0 });
                    if (element != null) element.RenderTransform = transformGroup;
                }

                //Callback to complete
                if (onPrintComplete != null)
                {
                    onPrintComplete();
                }
            };

            printDocument.Print(document);
        }
    }
}
