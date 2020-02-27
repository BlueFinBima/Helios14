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

namespace GadrocsWorkshop.Helios.Controls
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System.Windows;


    /// <summary>
    /// Interaction logic for CustomDrumAppearanceEditor.xaml
    /// </summary>
    [HeliosPropertyEditor("Helios.Base.CustomDrum", "Appearance")]
    public partial class CustomDrumAppearanceEditor : HeliosPropertyEditor
    {
        public CustomDrumAppearanceEditor()
        {
            InitializeComponent();
        }

        private void OnClick (object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.Reset();
			}
		}
		private void IncInitV(object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.InitialVertical = drum.InitialVertical + 1;
			}
		}

		private void DecInitV(object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.InitialVertical = drum.InitialVertical - 1;
			}
		}
		private void IncMinV(object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.MinVertical = drum.MinVertical + 1;
			}
		}

		private void DecMinV(object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.MinVertical = drum.MinVertical - 1;
			}
		}

		private void IncMaxV(object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.VerticalTravel = drum.VerticalTravel + 1;
			}
		}

		private void DecMaxV(object sender, RoutedEventArgs e)
		{
			CustomDrum drum = Control as CustomDrum;
			if (drum != null)
			{
				drum.VerticalTravel = drum.VerticalTravel - 1;
			}
		}

        private void HeliosTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void ImagePicker_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
