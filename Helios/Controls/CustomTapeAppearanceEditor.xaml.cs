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
    /// Interaction logic for CustomTapeAppearanceEditor.xaml
    /// </summary>
    [HeliosPropertyEditor("Helios.Base.CustomTape", "Appearance")]
    public partial class CustomTapeAppearanceEditor : HeliosPropertyEditor
    {
        public CustomTapeAppearanceEditor()
        {
            InitializeComponent();
        }

       

		private void OnClick (object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.Reset();
			}
		}

		private void IncInitH(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.InitialHorizontal = tape.InitialHorizontal + 1;
			}
		}

		private void DecInitH(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.InitialHorizontal = tape.InitialHorizontal - 1;
			}
		}
		private void IncMinH(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.MinHorizontal = tape.MinHorizontal + 1;
			}
		}

		private void DecMinH(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.MinHorizontal = tape.MinHorizontal - 1;
			}
		}

		private void IncMaxH (object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.HorizontalTravel = tape.HorizontalTravel+1;
			}
		}

		private void DecMaxH (object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.HorizontalTravel = tape.HorizontalTravel - 1;
			}
		}



		private void IncInitV(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.InitialVertical = tape.InitialVertical + 1;
			}
		}

		private void DecInitV(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.InitialVertical = tape.InitialVertical - 1;
			}
		}
		private void IncMinV(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.MinVertical = tape.MinVertical + 1;
			}
		}

		private void DecMinV(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.MinVertical = tape.MinVertical - 1;
			}
		}

		private void IncMaxV(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.VerticalTravel = tape.VerticalTravel + 1;
			}
		}

		private void DecMaxV(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.VerticalTravel = tape.VerticalTravel - 1;
			}
		}






		private void IncInitR(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.InitialRotation = tape.InitialRotation + 1;
			}
		}

		private void DecInitR(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.InitialRotation = tape.InitialRotation - 1;
			}
		}
		private void IncMinR(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.MinRotation = tape.MinRotation + 1;
			}
		}

		private void DecMinR(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.MinRotation = tape.MinRotation - 1;
			}
		}

		private void IncMaxR(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.RotationTravel = tape.RotationTravel + 1;
			}
		}

		private void DecMaxR(object sender, RoutedEventArgs e)
		{
			CustomTape tape = Control as CustomTape;
			if (tape != null)
			{
				tape.RotationTravel = tape.RotationTravel - 1;
			}
		}

	
	}
}
