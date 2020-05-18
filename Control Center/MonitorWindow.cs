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

namespace GadrocsWorkshop.Helios.ControlCenter
{
    using GadrocsWorkshop.Helios.Windows.Controls;

    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;

    public class MonitorWindow : Window
    {
        private Monitor _display;
        private bool _needsDisplay;
        private bool _autoInvalidate;
        private HeliosVisualView _viewer;
        private WindowInteropHelper _helper;

        //private HeliosVisual _touchDownControl;

        public MonitorWindow(Monitor monitor)
            : this(monitor, false, true)
        {
        }

        public MonitorWindow(Monitor monitor, bool highQualityBitmapScaling, bool autoInvalidate)
        {
            _viewer = new HeliosVisualView();
            _viewer.HighQualityBitmapScaling = highQualityBitmapScaling;
            _viewer.Visual = monitor;
            Content = _viewer;

            _display = monitor;
            _autoInvalidate = autoInvalidate;
            _needsDisplay = true;

            Top = _display.Top;
            Left = _display.Left;
            Width = _display.Width;
            Height = _display.Height;

            ShowInTaskbar = false;
            WindowStyle = System.Windows.WindowStyle.None;
            (Monitor.Renderer as MonitorRenderer).SkipFill = true;

            if (!Monitor.FillBackground)
            {
                ConfigManager.LogManager.LogDebug("Setting " + monitor.Name + " to transparent.");
                AllowsTransparency = true;
                Background = Brushes.Transparent;
            }
            else
            {
                if (Monitor.BackgroundColor.A != 255)
                {
                    ConfigManager.LogManager.LogDebug("Setting " + monitor.Name + " to transparent.");
                    AllowsTransparency = true;
                }
                Background = new SolidColorBrush(Monitor.BackgroundColor);
            }

            Focusable = false;

            if (Monitor.AlwaysOnTop)
            {
                Topmost = true;
            }

            ResizeMode = System.Windows.ResizeMode.NoResize;
        }

        #region Properties

        public IntPtr Handle
        {
            get
            {
                if (_helper != null)
                {
                    return _helper.Handle;
                }
                else
                {
                    return IntPtr.Zero;
                }
            }
        }

        public bool NeedsDisplayUpdate
        {
            get
            {
                return _needsDisplay;
            }
        }

        #endregion

        public Monitor Monitor
        {
            get { return _display; }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            _helper = new WindowInteropHelper(this);
            NativeMethods.SetWindowLong(_helper.Handle,
                NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(_helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);

            try
            {
                HwndSource hwndSource = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
                if (null == hwndSource)
                {
                    // REVISIT find out why this happens when screen is high DPI
                    ConfigManager.LogManager.LogInfo("Cannot determine rendering mode for " + _display.Name);
                    return;
                }
                HwndTarget hwndTarget = hwndSource.CompositionTarget;
                if (hwndTarget.RenderMode == RenderMode.SoftwareOnly)
                {
                    ConfigManager.LogManager.LogWarning(_display.Name + " is rendering in software mode, expect high CPU usage.");
                }
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogWarning("Error trying to determine rendering mode for " + _display.Name, ex);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            //source.AddHook(WndProc);
        }

        private HeliosVisual ControlAt(HeliosVisual visual, Point location)
        {
            if (!visual.IsHidden)
            {
                Point localLocation = location;
                switch (visual.Rotation)
                {
                    case HeliosVisualRotation.CW:
                        localLocation.X = location.Y;
                        localLocation.Y = visual.Height - location.X;
                        break;

                    case HeliosVisualRotation.CCW:
                        localLocation.X = visual.Width - location.Y;
                        localLocation.Y = location.X;
                        break;
                }

                foreach (HeliosVisual child in visual.Children.Reverse())
                {
                    if (!child.IsHidden && child.DisplayRectangle.Contains(localLocation))
                    {
                        localLocation.X -= child.Left;
                        localLocation.Y -= child.Top;
                        HeliosVisual childHit = ControlAt(child, localLocation);
                        if (childHit != null)
                        {
                            return childHit;
                        }
                    }
                }

                if (visual.HitTest(localLocation))
                {
                    return visual;
                }
            }
            return null;
        }

        private Point GetLocationForControl(HeliosVisual visual, Point location)
        {
            Point startLocation;

            HeliosVisual parent = visual.Parent;
            if (parent != null && !(parent is Monitor))
            {
                startLocation = GetLocationForControl(parent, location);
            }
            else
            {
                startLocation = location;
            }

            Point controlLocation = startLocation;
            switch (visual.Rotation)
            {
                case HeliosVisualRotation.CW:
                    controlLocation.X = controlLocation.Y;
                    controlLocation.Y = visual.Height - controlLocation.X;
                    break;

                case HeliosVisualRotation.CCW:
                    controlLocation.X = visual.Width - controlLocation.Y;
                    controlLocation.Y = location.X;
                    break;
            }

            controlLocation.X -= visual.Left;
            controlLocation.Y -= visual.Top;

            return controlLocation;
        }

        //protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        //{
        //    Point location = e.GetPosition(this);
        //    _touchDownControl = ControlAt(_display, location);
        //    if (_touchDownControl != null)
        //    {
        //        e.MouseDevice.Capture(this);
        //        _touchDownControl.MouseDown(GetLocationForControl(_touchDownControl, location));
        //        e.Handled = true;
        //    }
        //}

        //protected override void OnPreviewMouseMove(MouseEventArgs e)
        //{
        //    if (_touchDownControl != null && e.MouseDevice.Captured == this)
        //    {
        //        Point location = GetLocationForControl(_touchDownControl, e.GetPosition(this));
        //        _touchDownControl.MouseDrag(location);
        //        e.Handled = true;
        //    }
        //}

        //protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        //{
        //    if (e.MouseDevice.Captured == this)
        //    {
        //        if (_touchDownControl != null)
        //        {
        //            Point location = GetLocationForControl(_touchDownControl, e.GetPosition(this));
        //            _touchDownControl.MouseUp(location);
        //            _touchDownControl = null;
        //        }
        //        e.MouseDevice.Capture(null);
        //        e.Handled = true;
        //    }
        //}

        //private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (msg == EGalaxTouch.EGALAXY_MESSAGE_ID)
        //    {
        //        TouchEvent touchEvent = EGalaxTouch.ProcessMessage(this, (uint)msg, wParam, lParam);
        //        ConfigManager.LogManager.LogInfo("Touch " + touchEvent.EventType.ToString() + " Event:" + touchEvent.Location.ToString());

        //        switch (touchEvent.EventType)
        //        {
        //            case TouchEventType.Down:
        //                _touchDownControl = ControlAt(_display, touchEvent.Location);
        //                if (_touchDownControl != null)
        //                {
        //                    Point adjustedLocation = GetLocationForControl(_touchDownControl, touchEvent.Location);
        //                    _touchDownControl.MouseDown(adjustedLocation);
        //                }
        //                break;
        //            case TouchEventType.Move:
        //                if (_touchDownControl != null)
        //                {
        //                    Point adjustedLocation = GetLocationForControl(_touchDownControl, touchEvent.Location);
        //                    _touchDownControl.MouseDrag(adjustedLocation);
        //                }
        //                break;
        //            case TouchEventType.Up:
        //                if (_touchDownControl != null)
        //                {
        //                    Point adjustedLocation = GetLocationForControl(_touchDownControl, touchEvent.Location);
        //                    _touchDownControl.MouseUp(adjustedLocation);
        //                    _touchDownControl = null;
        //                }
        //                break;
        //        }

        //        handled = true;
        //    }
        //    return IntPtr.Zero;
        //}
    }
}
