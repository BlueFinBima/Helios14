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

using System.ComponentModel;
using System.Windows;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.ComponentModel;
using net.derammo.Helios.SampleProgram.Sample;

namespace net.derammo.Helios.SampleProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // create some sample model we have
            SampleModel sampleModel = new SampleModel();
            sampleModel.PropertyChanged += SampleModel_PropertyChanged;

            // we explicitly create the view model for now instead of automatically selecting it
            SampleData = new SampleViewModel(sampleModel);

            // we are our own data context, because our child will just bind to "SampleData"
            DataContext = this;

            InitializeComponent();
        }

        private void SampleModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this is what Profile Editor does for us, but we aren't in the Profile Editor,
            // so we implement it here
            if (e is PropertyNotificationEventArgs args)
            {
                ConfigManager.UndoManager?.AddPropertyChange(sender, args);
            }
        }

        public SampleViewModel SampleData { get; }
    }
}