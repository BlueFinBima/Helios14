// Copyright 2020 Ammo Goettsch
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

using System.Windows;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// the Dialog.ShowModal command requires an instance of this to be the parameter
    /// </summary>
    public class ShowModalParameter : FrameworkElement
    {
        /// <summary>
        /// the content which will be displayed in the dialog
        /// </summary>
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(ShowModalParameter),
                new PropertyMetadata(null));

        /// <summary>
        /// data template to use, or null if data template should be
        /// resolved via search for a DataTemplate resource from the
        /// point of origin of the ShowModal event, based on the
        /// type of Content
        /// </summary>
        public DataTemplate DataTemplate
        {
            get => (DataTemplate) GetValue(DataTemplateProperty);
            set => SetValue(DataTemplateProperty, value);
        }

        public static readonly DependencyProperty DataTemplateProperty =
            DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(ShowModalParameter),
                new PropertyMetadata(null));
    }
}