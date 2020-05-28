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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    /// <summary>
    /// Splash screen for Helios, currently also used as about box without any buttons
    /// </summary>
    public partial class About : Window
    {
        // these are in order of being primary on the project
        public static readonly string[] Authors = { "Gadroc", "BlueFinBima", "derammo" };

        // these will be listed alphabetically from now on
        public static string[] Contributors =>
            ContributorsArray.OrderBy(name => name, StringComparer.InvariantCultureIgnoreCase).ToArray();

        // raw contributor list, sorted when displayed
        private static readonly string[] ContributorsArray = 
        {
            "BeamRider",
            "CaptZeen",
            "Cylution",
            "damien022",
            "Jabbers",
            "KiwiLostInMelb",
            "Phar71",
            "Rachmaninoff",
            "wheelchock",
            "Will Hartsell",
            "WillianG83",
            "yzfanimal",
            "ZoeESummers",  // for UX work
            "ertiyu",       // for testing
            "Polaris",      // for testing
            "Sliceback"     // for testing
        };

        public About()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            Version runningVersion = VersionChecker.RunningVersion;
            VersionBlock.Text = runningVersion.Major.ToString() + "." + runningVersion.Minor.ToString() + "." + runningVersion.Build.ToString() + "." + runningVersion.Revision.ToString("0000");
            AuthorsBlock.Text = string.Join("; ", Authors);
            ContributionBlock.Text = string.Join("; ", Contributors);
            StatusBlock.Text = RunningVersion.IsDevelopmentPrototype ? "Development Prototype" : "Released";
            ForkBlock.Text = $"Fork: {KnownLinks.GitRepoUrl()}";
            base.OnActivated(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Close();
            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Close();
            base.OnMouseDown(e);
        }
    }
}
