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
    using System.Windows;
    using System.Windows.Media;

    public class CustomGaugeRenderer : HeliosVisualRenderer
    {
        private ImageSource _image, _bgplate_image;
        private ImageBrush _brush;
        private Rect _imageRect, _bgplate_imageRect;
        private Point _center, _punto;
        private Brush _scopeBrush;

       

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
        _scopeBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        Pen _scopePen = new Pen(_scopeBrush, 1d);


            CustomNeedle customNeedle = Visual as CustomNeedle;
            if (customNeedle != null)
            {
                drawingContext.DrawImage(_bgplate_image, _bgplate_imageRect);

                drawingContext.PushTransform(new RotateTransform(customNeedle.KnobRotation, _center.X, _center.Y));
                drawingContext.DrawImage(_image, _imageRect);
                drawingContext.DrawLine(_scopePen, _center, _punto); //draw rotation point for reference
                drawingContext.Pop();
            }
        }

        protected override void OnRefresh()
        {

           
            CustomGauge customGauge = Visual as CustomGauge;  // ----------BG plate
            CustomNeedle customNeedle = Visual as CustomNeedle;   // ----- needle

			

			if (customNeedle != null) // needle
            {
               
                _imageRect.X = customNeedle.Width * customGauge.Needle_PosX; ;
                _imageRect.Y = customNeedle.Height * customGauge.Needle_PosY; ;
                _image = ConfigManager.ImageManager.LoadImage(customNeedle.KnobImage);
				if (_image == null)
				{
					_image = ConfigManager.ImageManager.LoadImage("{Helios}/Images/General/missing_image.png"); 
				}
				_imageRect.Height = customGauge.Height * customGauge.Needle_Scale;
                _imageRect.Width = (_image.Width * (customGauge.Height/_image.Height) )*customGauge.Needle_Scale; // uniform image based on Height
                _brush = new ImageBrush(_image);
                _center = new Point(customGauge.Width * customGauge.Needle_PivotX, customGauge.Height * customGauge.Needle_PivotY); // calculate rotation point
                _punto = new Point((customNeedle.Width * customGauge.Needle_PivotX)+1, customNeedle.Height * customGauge.Needle_PivotY);

            }
            else
            {
                _image = null;
                _brush = null;
            }
            if (customGauge != null) // plate
            {
                _bgplate_imageRect.Width = customNeedle.Width;
                _bgplate_imageRect.Height = customNeedle.Height;
                _bgplate_image = ConfigManager.ImageManager.LoadImage(customGauge.BGPlateImage);
            }
            else
            {
                _bgplate_image = null;
                _brush = null;
            }
        }
    }
}
