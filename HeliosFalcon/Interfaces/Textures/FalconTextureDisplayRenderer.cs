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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.interfaces.Textures
{
    class FalconTextureDisplayRenderer : HeliosVisualRenderer
    {
        private FalconTextureDisplay _display;
        private ImageBrush _defaultImage;
        private Rect _displayRect = new Rect(0, 0, 0, 0);

        protected override void OnPropertyChanged(PropertyNotificationEventArgs args)
        {
            if (args.PropertyName.Equals("Visual"))
            {
                _display = args.NewValue as FalconTextureDisplay;
                OnRefresh();
            }
            base.OnPropertyChanged(args);
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (_display != null)
            {
                if (_display.TextureMemory != null && _display.TextureMemory.IsDataAvailable)
                {
                    drawingContext.DrawRectangle(CreateImageBrush(), null, _displayRect);
                }
                else if (_display.IsRunning)
                {
                    drawingContext.DrawRectangle(Brushes.Black, null, _displayRect);
                }
                else if (_defaultImage != null)
                {
                    drawingContext.DrawRectangle(_defaultImage, null, _displayRect);
                }
            }
        }

        private ImageBrush CreateImageBrush()
        {
            ImageBrush brush = null;
            if (_display.TextureMemory.IsDataAvailable)
            {
                NativeMethods.DDSURFACEDESC2 surfaceDesc = (NativeMethods.DDSURFACEDESC2)_display.TextureMemory.MarshalTo(typeof(NativeMethods.DDSURFACEDESC2), 4);
                PixelFormat format = PixelFormats.Bgr32;  //.Format32bppRgb;
                switch (surfaceDesc.ddpfPixelFormat.dwRGBBitCount)
                {
                    case 16:
                        format = PixelFormats.Bgr555; // PixelFormat.Format16bppRgb555;
                        break;
                    case 24:
                        format = PixelFormats.Bgr24; // PixelFormat.Format24bppRgb;
                        break;
                    case 32:
                        format = PixelFormats.Bgr32; // PixelFormat.Format32bppRgb;
                        break;
                }

                int offset = (surfaceDesc.lPitch * (int)_display.TextureRect.Y) + ((int)_display.TextureRect.X * (surfaceDesc.ddpfPixelFormat.dwRGBBitCount / 8));

                BitmapSource image = BitmapSource.Create((int)_display.TextureRect.Width,
                                                         (int)_display.TextureRect.Height,
                                                         96,
                                                         96,
                                                         format,
                                                         null,
                                                         _display.TextureMemory.GetPointer(offset + 4 + surfaceDesc.dwSize),
                                                         surfaceDesc.lPitch * (int)_display.TextureRect.Height,
                                                         surfaceDesc.lPitch);

                brush = new ImageBrush(image);
                _defaultImage = new ImageBrush(image);
                _defaultImage.Stretch = Stretch.Fill;
                _defaultImage.TileMode = TileMode.None;
                _defaultImage.Viewport = new Rect(0d, 0d, 1d, 1d);
                _defaultImage.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            }
            return brush;
        }

        protected override void OnRefresh()
        {
            if (_display != null)
            {
                ImageSource image = ConfigManager.ImageManager.LoadImage(_display.DefaultImage);
                if (image == null)
                {
                    _defaultImage = null;
                }
                else
                {
                    _defaultImage = new ImageBrush(image);
                    _defaultImage.Stretch = Stretch.Fill;
                    _defaultImage.TileMode = TileMode.None;
                    _defaultImage.Viewport = new Rect(0d, 0d, 1d, 1d);
                    _defaultImage.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
                }
                _displayRect.Width = _display.Width;
                _displayRect.Height = _display.Height;
            }
            else
            {
                _defaultImage = null;
            }
        }
    }
}
