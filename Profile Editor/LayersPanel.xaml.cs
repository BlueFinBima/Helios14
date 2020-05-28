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

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using GadrocsWorkshop.Helios.ProfileEditor.ViewModel;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for LayersPanel.xaml
    /// </summary>
    public partial class LayersPanel : UserControl
    {
        private VisualsListItemCollections _visuals = new VisualsListItemCollections();
        private HeliosVisualContainerEditor _editor;

        public LayersPanel()
        {
            DataContext = this;
            Focusable = false;
            InitializeComponent();
        }

        #region Properties

        public VisualsListItemCollections Controls
        {
            get { return _visuals; }
        }

        public HeliosEditorDocument Editor
        {
            get { return (HeliosEditorDocument)GetValue(EditorProperty); }
            set { SetValue(EditorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Editor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditorProperty =
            DependencyProperty.Register("Editor", typeof(HeliosEditorDocument), typeof(LayersPanel), new PropertyMetadata(null, EditorChanged));

        #endregion

        private static void EditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LayersPanel p = d as LayersPanel;
            p.UpdateEditor();
        }

        private void UpdateEditor()
        {
            if (_editor != null)
            {
                _editor.VisualContainer.Children.CollectionChanged -= Controls_CollectionChanged;
                _editor.SelectedItems.CollectionChanged -= SelectedControls_CollectionChanged;
            }            

            if (Editor is PanelDocument)
            {
                _editor = (Editor as PanelDocument).PanelEditor;
            }
            else if (Editor is MonitorDocument)
            {
                _editor = (Editor as MonitorDocument).MonitorEditor;
            }
            else
            {
                _editor = null;
            }

            if (_editor != null)
            {
                _editor.VisualContainer.Children.CollectionChanged += Controls_CollectionChanged;
                _editor.SelectedItems.CollectionChanged += SelectedControls_CollectionChanged;
            }

            CopyControlsList();
        }

        private void Controls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)CopyControlsList);
        }

        private void SelectedControls_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (HeliosVisual control in e.OldItems)
                {
                    if ((e.NewItems == null || !e.NewItems.Contains(control)) && _visuals.ContainsKey(control))
                    {
                        _visuals[control].IsSelected = false;
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosVisual control in e.NewItems)
                {
                    if (_visuals.ContainsKey(control))
                    {
                        _visuals[control].IsSelected = true;
                    }
                }
            }
        }

        private void CopyControlsList()
        {
            _visuals.Clear();

            if (_editor != null)
            {
                foreach (HeliosVisual control in _editor.VisualContainer.Children.Reverse())
                {
                    _visuals.Add(new VisualsListItem(control, _editor.SelectedItems.Contains(control)));
                }
            }
        }

        private void ItemLockChecked(object sender, RoutedEventArgs e)
        {
            FrameworkElement senderControl = sender as FrameworkElement;
            if (senderControl != null)
            {
                HeliosVisual control = (HeliosVisual)senderControl.Tag;
                if (control != null && _editor != null && _editor.SelectedItems.Contains(control))
                {
                    _editor.SelectedItems.Remove(control);
                }
            }
        }

        private void ControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement senderControl = sender as FrameworkElement;
            if (senderControl != null)
            {
                HeliosVisual control = (HeliosVisual)senderControl.Tag;
                if (control != null && _editor != null && !control.IsLocked)
                {
                    if (_editor.SelectedItems.Contains(control))
                    {
                        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                        {
                            _editor.SelectedItems.Remove(control);
                        }
                        else
                        {
                            _editor.SelectedItems.Clear();
                            _editor.SelectedItems.Add(control);
                        }
                    }
                    else
                    {
                        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                        {
                            _editor.SelectedItems.Clear();
                        }
                        _editor.SelectedItems.Add(control);
                    }
                    _editor.Focus();
                }
            }
            
        }
    }
}
