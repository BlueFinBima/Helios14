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
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Windows.Controls;

namespace GadrocsWorkshop.Helios.ProfileEditor.PropertyEditors
{
    /// <summary>
    /// base Layout editor, which is the top-most editor shown for all visual controls
    /// </summary>
    [HeliosPropertyEditor("*", "hply")]
    public partial class LayoutPropertyEditor : HeliosPropertyEditor, IDataErrorInfo
    {
        public LayoutPropertyEditor()
        {
            InitializeComponent();
        }

        #region Properties

        public override string Category => "Layout";

        public string VisualName
        {
            get => (string) GetValue(VisualNameProperty);
            set => SetValue(VisualNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for VisualName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisualNameProperty =
            DependencyProperty.Register("VisualName", typeof(string), typeof(LayoutPropertyEditor),
                new PropertyMetadata(""));

        /// <summary>
        /// the friendly name of the HeliosControl type or null
        /// </summary>
        public string ControlTypeName
        {
            get => (string) GetValue(ControlTypeNameProperty);
            set => SetValue(ControlTypeNameProperty, value);
        }

        public static readonly DependencyProperty ControlTypeNameProperty =
            DependencyProperty.Register("ControlTypeName", typeof(string), typeof(LayoutPropertyEditor),
                new PropertyMetadata(null));

        #endregion

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ControlProperty)
            {
                VisualName = Control.Name;
                LoadControlTypeName();
            }

            if (e.Property == VisualNameProperty)
            {
                if ((this as IDataErrorInfo)["VisualName"] == null)
                {
                    Control.Name = VisualName;
                }
            }

            base.OnPropertyChanged(e);
        }

        private void LoadControlTypeName()
        {
            Type controlType = Control.GetType();
            foreach (object attribute in controlType.GetCustomAttributes(false))
            {
                if (!(attribute is HeliosControlAttribute controlAttribute))
                {
                    continue;
                }

                ControlTypeName = controlAttribute.Name;
                return;
            }

            ControlTypeName = null;
        }

        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (!columnName.Equals("VisualName"))
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(VisualName))
                {
                    return "Name cannot be blank.";
                }

                if (!Regex.IsMatch(VisualName, "^[a-zA-Z0-9_ ]*$"))
                {
                    return "Name must not contain special characters.";
                }

                if (Control != null && Control.Parent != null && !VisualName.Equals(Control.Name) &&
                    Control.Parent.Children.ContainsKey(VisualName))
                {
                    return "Name must be unique.";
                }

                return null;
            }
        }
    }
}