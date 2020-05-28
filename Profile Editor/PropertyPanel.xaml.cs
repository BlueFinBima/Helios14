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
    using System.Windows;
    using System.Windows.Controls;
    
    /// <summary>
    /// Interaction logic for PropertyPanel.xaml
    /// </summary>
    public partial class PropertyPanel : UserControl
    {
        public PropertyPanel()
        {
            DataContext = this;
            InitializeComponent();
            PropertyEditorGroups = new PropertyEditorGroupCollection();
        }

        #region Properties

        public HeliosPropertyEditorCollection PropertyEditors
        {
            get { return (HeliosPropertyEditorCollection)GetValue(PropertyEditorsProperty); }
            set { SetValue(PropertyEditorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyEditors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyEditorsProperty =
            DependencyProperty.Register("PropertyEditors", typeof(HeliosPropertyEditorCollection), typeof(PropertyPanel), new PropertyMetadata(null));

        public PropertyEditorGroupCollection PropertyEditorGroups
        {
            get { return (PropertyEditorGroupCollection)GetValue(PropertyEditorGroupsProperty); }
            set { SetValue(PropertyEditorGroupsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyEditorGroups.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyEditorGroupsProperty =
            DependencyProperty.Register("PropertyEditorGroups", typeof(PropertyEditorGroupCollection), typeof(PropertyPanel), new PropertyMetadata(null));

        #endregion

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == PropertyEditorsProperty)
            {
                HeliosPropertyEditorCollection oldEditors = e.OldValue as HeliosPropertyEditorCollection;
                if (oldEditors != null)
                {
                    oldEditors.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PropertyEditors_CollectionChanged);
                }

                PropertyEditorGroups.Clear();

                if (PropertyEditors != null)
                {
                    LoadGroups();
                    PropertyEditors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(PropertyEditors_CollectionChanged);
                }
            }
            base.OnPropertyChanged(e);
        }

        void PropertyEditors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PropertyEditorGroups.Clear();
            LoadGroups();
        }

        private void LoadGroups()
        {
            if (PropertyEditors != null)
            {
                foreach (HeliosPropertyEditor editor in PropertyEditors)
                {
                    PropertyEditorGroups.Add(new PropertyEditorGroup(editor));

                }
            }
        }
    }
}
