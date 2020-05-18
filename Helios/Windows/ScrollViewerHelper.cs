// Copyright 2020 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// https://serialseb.com/blog/2007/09/03/wpf-tips-6-preventing-scrollviewer-from/
    /// this behavior class keeps ScrollsViewer from capturing the scroll interaction (mouse wheel for example) when it doesn't
    /// need
    /// to scroll, and lets its ancestor scrollers scroll instead
    /// </summary>
    public class ScrollViewerHelper
    {
        public static bool GetFixScrolling(DependencyObject obj) => (bool) obj.GetValue(FixScrollingProperty);

        public static void SetFixScrolling(DependencyObject obj, bool value)
        {
            obj.SetValue(FixScrollingProperty, value);
        }

        public static readonly DependencyProperty FixScrollingProperty =
            DependencyProperty.RegisterAttached("FixScrolling", typeof(bool), typeof(ScrollViewerHelper),
                new FrameworkPropertyMetadata(false, OnFixScrollingPropertyChanged));

        public static void OnFixScrollingPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (viewer == null)
            {
                throw new ArgumentException(@"The dependency property can only be attached to a ScrollViewer",
                    nameof(sender));
            }

            if ((bool) e.NewValue)
            {
                viewer.PreviewMouseWheel += HandlePreviewMouseWheel;
            }
            else if ((bool) e.NewValue == false)
            {
                viewer.PreviewMouseWheel -= HandlePreviewMouseWheel;
            }
        }

        private static readonly List<MouseWheelEventArgs> ReentrantList = new List<MouseWheelEventArgs>();

        private static void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollControl = sender as ScrollViewer;
            if (e.Handled || sender == null || ReentrantList.Contains(e))
            {
                return;
            }

            MouseWheelEventArgs previewEventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.PreviewMouseWheelEvent,
                Source = sender
            };
            UIElement originalSource = e.OriginalSource as UIElement;
            ReentrantList.Add(previewEventArg);
            originalSource?.RaiseEvent(previewEventArg);
            ReentrantList.Remove(previewEventArg);

            // at this point if no one else handled the event in our children, we do our job

            if (scrollControl == null)
            {
                // don't have a control
                return;
            }

            if (previewEventArg.Handled)
            {
                // already handled
                return;
            }

            bool scrollerAtBoundary;
            if (e.Delta > 0)
            {
                scrollerAtBoundary = Math.Abs(scrollControl.VerticalOffset) < 0.000001;
            }
            else
            {
                scrollerAtBoundary = scrollControl.VerticalOffset >=
                                     scrollControl.ExtentHeight - scrollControl.ViewportHeight;
            }

            if (!scrollerAtBoundary)
            {
                // we can still scroll, let the event happen normally
                return;
            }

            e.Handled = true;
            MouseWheelEventArgs eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            UIElement parent = (UIElement) ((FrameworkElement) sender).Parent;
            parent?.RaiseEvent(eventArg);
        }
    }
}