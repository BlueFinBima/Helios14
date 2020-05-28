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

using System.Windows.Input;
using System.Windows.Threading;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using GadrocsWorkshop.Helios.Controls;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for PanelDocument.xaml
    /// </summary>
    public partial class PanelDocument : VisualContainerEditorDocument
    {
        public PanelDocument()
        {
            InitializeComponent();

            PanelEditor.SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            PreviewMouseWheel += Window_PreviewMouseWheel;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            const double zoomChange = 0.25;
            if (e.Delta > 0)
            {
                if (!(PanelEditor.ZoomLevel < 4d))
                {
                    return;
                }

                PanelEditor.ZoomLevel = Math.Min(PanelEditor.ZoomLevel + zoomChange, 4d);
                e.Handled = true;
            }
            else if (e.Delta < 0)
            {
                if (!(PanelEditor.ZoomLevel > -4d))
                {
                    return;
                }

                PanelEditor.ZoomLevel = Math.Max(PanelEditor.ZoomLevel - zoomChange, -4d);
                e.Handled = true;
            }
        }

        public PanelDocument(HeliosPanel panel)
            : this()
        {
            Panel = panel;
        }

        #region Properties

        public HeliosPanel Panel
        {
            get { return (HeliosPanel)GetValue(PanelProperty); }
            set { SetValue(PanelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Panel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PanelProperty =
            DependencyProperty.Register("Panel", typeof(HeliosPanel), typeof(PanelDocument), new PropertyMetadata(null, new PropertyChangedCallback(PanelPropertyChanged)));

        public override string Title
        {
            get { return Panel.Name; }
        }

        #endregion

        protected static void PanelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanelDocument p = d as PanelDocument;
            p.BindingFocus = (HeliosPanel)e.NewValue;
        }

        void SelectedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (PanelEditor.SelectedItems.Count == 0)
            {
                BindingFocus = Panel;
            }
            else if (PanelEditor.SelectedItems.Count == 1)
            {
                BindingFocus = PanelEditor.SelectedItems[0];
            }
            else
            {
                BindingFocus = null;
            }
        }

        public override void SetBindingFocus(HeliosObject bindingFoucsObject)
        {
            HeliosVisual visual = bindingFoucsObject as HeliosVisual;
            if (visual != null && Panel.Children.Contains(visual))
            {
                PanelEditor.SelectedItems.Clear();

                HeliosVisualView view = PanelEditor.GetViewerForVisual(visual);
                if (view != null)
                {
                    PanelEditor.UpdateLayout();
                    PanelEditor.SelectedItems.Add((HeliosVisual)bindingFoucsObject);
                    Dispatcher.BeginInvoke(new Action(view.BringIntoView));
                }
            }
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            PanelEditor.Focus();
        }
    }
}
