//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for MonitorDocument.xaml
    /// </summary>
    public partial class MonitorDocument : VisualContainerEditorDocument
    {
        public MonitorDocument()
        {
            InitializeComponent();

            MonitorEditor.SelectedItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(SelectedItems_CollectionChanged);
            PreviewMouseWheel += Window_PreviewMouseWheel;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            const double zoomChange = 0.25;
            if (e.Delta > 0)
            {
                if (!(MonitorEditor.ZoomLevel < 4d))
                {
                    return;
                }

                MonitorEditor.ZoomLevel = Math.Min(MonitorEditor.ZoomLevel + zoomChange, 4d);
                e.Handled = true;
            }
            else if (e.Delta < 0)
            {
                if (!(MonitorEditor.ZoomLevel > -4d))
                {
                    return;
                }

                MonitorEditor.ZoomLevel = Math.Max(MonitorEditor.ZoomLevel - zoomChange, -4d);
                e.Handled = true;
            }
        }

        public MonitorDocument(Monitor monitor) : this()
        {
            Monitor = monitor;
        }

        #region Properties

        public Monitor Monitor
        {
            get { return (Monitor)GetValue(MonitorProperty); }
            set { SetValue(MonitorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Monitor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MonitorProperty =
            DependencyProperty.Register("Monitor", typeof(Monitor), typeof(MonitorDocument), new PropertyMetadata(null, new PropertyChangedCallback(MonitorPropertyChanged)));

        public override string Title
        {
            get { return Monitor.Name; }
        }

        #endregion

        protected static void MonitorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MonitorDocument p = d as MonitorDocument;
            p.BindingFocus = e.NewValue as Monitor;
        }

        void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (MonitorEditor.SelectedItems.Count == 0)
            {
                BindingFocus = Monitor;
            }
            else if (MonitorEditor.SelectedItems.Count == 1)
            {
                BindingFocus = MonitorEditor.SelectedItems[0];
            }
            else
            {
                BindingFocus = null;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            MonitorEditor.Focus();
        }
    }
}
