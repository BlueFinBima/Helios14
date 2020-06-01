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

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Interaction logic for PreviewContent.xaml
    /// </summary>
    public partial class PreviewContent : UserControl
    {
        private static CalibrationPointCollectionDouble _zoomCalibration;

        static PreviewContent()
        {
            _zoomCalibration = new CalibrationPointCollectionDouble(-10d, 0.1d, 2d, 2d);
            _zoomCalibration.Add(new CalibrationPointDouble(0d, 1d));
        }

        public PreviewContent()
        {
            DataContext = this;
            InitializeComponent();
        }

        #region Properties

        public HeliosProfile Profile
        {
            get { return (HeliosProfile)GetValue(ProfileProperty); }
            set { SetValue(ProfileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Profile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(HeliosProfile), typeof(PreviewContent), new FrameworkPropertyMetadata(null));

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(PreviewContent), new UIPropertyMetadata(0d, new PropertyChangedCallback(OnZoomLevelChanged)));

        public bool Zoom
        {
            get { return (bool)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Zooom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(bool), typeof(PreviewContent), new PropertyMetadata(false, new PropertyChangedCallback(OnZoomChanged)));

        #endregion         

        public static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PreviewContent pc = (PreviewContent)d;
            pc.Preview.ZoomFactor = _zoomCalibration.Interpolate((double)e.NewValue);
        }

        public static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PreviewContent pc = (PreviewContent)d;
            
            double zoomFactor = Math.Min(pc.PreviewScroller.ActualWidth / pc.Profile.Monitors.VirtualScreenWidth, pc.PreviewScroller.ActualHeight / pc.Profile.Monitors.VirtualScreenHeight);

            if ((bool)e.NewValue)
            {
                pc.PreviewScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                pc.PreviewScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                pc.Preview.ZoomFactor = 1d;
                pc.ZoomPanel.Visibility = Visibility.Visible;

                DoubleAnimation da = new DoubleAnimation();
                da.From = zoomFactor;
                da.To = 1d;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.75));
                da.FillBehavior = FillBehavior.Stop;
                pc.Preview.BeginAnimation(ProfilePreview.ZoomFactorProperty, da);
            }
            else
            {
                pc.ZoomPanel.Visibility = Visibility.Collapsed;

                DoubleAnimation da = new DoubleAnimation();
                da.From = pc.Preview.ZoomFactor;
                da.To = zoomFactor;
                da.Duration = new Duration(TimeSpan.FromSeconds(0.75));
                da.Completed += new EventHandler(pc.Zoom_Completed);
                da.FillBehavior = FillBehavior.Stop;
                pc.Preview.BeginAnimation(ProfilePreview.ZoomFactorProperty, da);
            }
        }

        public void Zoom_Completed(object sender, EventArgs e)
        {
            ZoomLevel = 0d;
            Preview.ZoomFactor = 1d;
            PreviewScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            PreviewScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }
    }
}
