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
using System.Collections.ObjectModel;
using System.Windows;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    /// <summary>
    /// This is the glue code to interact with an editable combobox in a WPF dialog
    /// </summary>
    public class EditableComboBoxModel : DependencyObject
    {
        // lambdas to be provided by the parent
        public Func<SortedSet<string>> LoadItemSet;
        public Func<string> GetDefaultValue;
        public Func<string> GetCurrentValue;
        public new Action<string> SetCurrentValue;
        public event EventHandler<NewItemAddedArgs> NewItemAdded;

        /// <summary>
        /// late start after populating all the lambdas
        /// </summary>
        public void Init()
        {
            _items = LoadItemSet();
            _itemsExport = new ObservableCollection<string>(_items);
            SetValue(ItemsSourcePropertyKey, _itemsExport);

            // default
            if (GetCurrentValue() != null)
            {
                AddItem(GetCurrentValue());
                Text = GetCurrentValue();
            }
            else
            {
                Text = GetDefaultValue();
            }

            // the default value is always a valid selection
            AddItem(GetDefaultValue());
        }

        public class NewItemAddedArgs : EventArgs
        {
            public string Value { get; }

            internal NewItemAddedArgs(string value)
            {
                Value = value;
            }
        }

        /// <summary>
        /// this Property is bound to the text content of the combobox to detect when the user enters
        /// a value that is not in the combobox list
        /// </summary>
        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableComboBoxModel),
                new PropertyMetadata(TextChanged));

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // if DependencyProperty access is selected for write, this gets called instead of Text.set
            string value = e.NewValue as string;
            EditableComboBoxModel model = d as EditableComboBoxModel;
            if (value == "")
            {
                // reset to default
                value = model.GetDefaultValue();
                model.SetCurrentValue(null);

                if (value != "")
                {
                    // recurse once, since we will presumably get called again
                    model.SetValue(TextProperty, value);
                }
            }
            else
            {
                model.AddItem(value);
            }
        }

        /// <summary>
        /// this is the currently selected item for the combobox
        /// </summary>
        public string SelectedItem
        {
            get
            {
                string configured = (string) GetValue(SelectedItemProperty);
                return configured ?? GetDefaultValue();
            }
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(string), typeof(EditableComboBoxModel),
                new PropertyMetadata(SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // if DependencyProperty access is selected for write, this gets called instead of Text.set
            string value = e.NewValue as string;
            EditableComboBoxModel model = d as EditableComboBoxModel;
            if (value == model.GetDefaultValue())
            {
                // unset to use default
                value = null;
            }

            model.SetCurrentValue(value);
        }

        /// <summary>
        /// this is the list of currently allowed values for the combobox
        /// </summary>
        public ObservableCollection<string> ItemsSource => (ObservableCollection<string>) GetValue(ItemsSourceProperty);

        public static readonly DependencyPropertyKey ItemsSourcePropertyKey = DependencyProperty.RegisterReadOnly(
            "ItemsSource", typeof(ObservableCollection<string>), typeof(EditableComboBoxModel),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ItemsSourceProperty = ItemsSourcePropertyKey.DependencyProperty;

        // customizable set of values for combo box
        private SortedSet<string> _items;

        // the same data in the only format that WPF can bind successfully at the moment
        private ObservableCollection<string> _itemsExport;

        /// <summary>
        /// add a value to the list of items, if not already there, and export the list in the format required for WPF to bind it
        /// </summary>
        /// <param name="value"></param>
        private void AddItem(string value)
        {
            if (_items.Contains(value))
            {
                return;
            }

            _items.Add(value);
            // this is very inefficient but almost never happens
            // so I will do this rather than include some sorted observable collection
            int index = 0;
            foreach (string item in _items)
            {
                if (item == value)
                {
                    break;
                }

                index++;
            }

            _itemsExport.Insert(index, value);
            NewItemAdded?.Invoke(this, new NewItemAddedArgs(value));
        }
    }
}