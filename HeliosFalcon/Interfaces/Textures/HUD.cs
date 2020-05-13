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

using System.Windows;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.Textures
{
    [HeliosControl("Helios.Falcon.HUD", "HUD", "Falcon Textures", typeof(FalconTextureDisplayRenderer))]
    public class HUD : FalconTextureDisplay
    {
        //TODO defaultRec values here were specific to OpenFalcon
        private static readonly Rect _defaultRect = new Rect(5, 5, 255, 255);

        public HUD()
            : base("HUD", new Size(255, 255))
        {
        }

        protected override FalconTextureDisplay.FalconTextures Texture
        {
            get { return FalconTextureDisplay.FalconTextures.HUD; }
        }

        internal override string DefaultImage
        {
            get { return "{HeliosFalcon}/Images/OpenFalcon/hud.png"; }
        }

        protected override Rect DefaultRect
        {
            get { return _defaultRect; }
        }
    }
}
