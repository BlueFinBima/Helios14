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

namespace GadrocsWorkshop.Helios.ProfileEditor.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MonitorResetItem : NotificationObject
    {
        private Monitor _oldMonitor;
        private int _oldId;
        private int _newMonitor;
        private bool _scale;
        private List<HeliosVisual> _controls;
        private double _oldWidth;
        private double _oldHeight;

        public MonitorResetItem(Monitor oldMonitor, int oldId, int newId)
        {
            _controls = new List<HeliosVisual>();
            _oldMonitor = oldMonitor;
            _oldId = oldId;
            _newMonitor = newId;
            _oldWidth = oldMonitor.Width;
            _oldHeight = oldMonitor.Height;
            _scale = true;
        }

        public List<HeliosVisual> Controls
        {
            get
            {
                return _controls;
            }
        }

        public Monitor OldMonitor
        {
            get
            {
                return _oldMonitor;
            }
        }

        public int NewMonitor
        {
            get
            {
                return _newMonitor;
            }
            set
            {
                if (!_newMonitor.Equals(value))
                {
                    int oldValue = _newMonitor;
                    _newMonitor = value;
                    OnPropertyChanged("NewMonitor", oldValue, value, false);
                }
            }
        }

        public bool Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (!_scale.Equals(value))
                {
                    bool oldValue = _scale;
                    _scale = value;
                    OnPropertyChanged("Scale", oldValue, value, false);
                }
            }
        }

        public void Reset()
        {
            Monitor display = ConfigManager.DisplayManager.Displays[_oldId];

            // change the size of the monitor to match the local display
            OldMonitor.Top = display.Top;
            OldMonitor.Left = display.Left;
            OldMonitor.Width = display.Width;
            OldMonitor.Height = display.Height;
            OldMonitor.Orientation = display.Orientation;

            // REVISIT: this does not invalidate the profile preview's image of the monitor
            // after the last height change, so it shows the wrong height (height of old monitor)

            // REVISIT: does this work if there is an orientation change?
            double scale = Math.Min(display.Width / _oldWidth, display.Height / _oldHeight);

            foreach (HeliosVisual visual in OldMonitor.Children)
            {
                if (Scale)
                {
                    ScaleControl(visual, scale);
                }
                else
                {
                    CheckBounds(visual, OldMonitor);
                }
            }
        }

        public void RemoveControls()
        {
            HeliosVisual[] children = OldMonitor.Children.ToArray();
            foreach (HeliosVisual visual in children)
            {
                _controls.Add(visual);
                OldMonitor.Children.Remove(visual);
            }
        }

        public void PlaceControls(Monitor newMonitor)
        {
            double scale = Math.Min(newMonitor.Width / _oldWidth, newMonitor.Height / _oldHeight);
            foreach (HeliosVisual visual in _controls)
            {
                // Make sure name is unique
                int i = 1;
                String name = visual.Name;
                while (newMonitor.Children.ContainsKey(name))
                {
                    name = visual.Name + " " + i++;
                }
                visual.Name = name;

                newMonitor.Children.Add(visual);

                if (Scale)
                {
                    ScaleControl(visual, scale);
                }
                else
                {
                    CheckBounds(visual, newMonitor);
                }
            }
        }

        public void CopySettings(Monitor newMonitor)
        {
            if (_controls.Count == 0)
            {
                // nothing transferred
                // NOTE: this also covers the case where the source and target monitor are the same
                return;
            }
            if (!OldMonitor.FillBackground)
            {
                // the transferred controls require a transparent monitor,
                // so we have to set this, even if it gets combined with controls
                // from opaque monitors
                newMonitor.FillBackground = false;
            }
            if (OldMonitor.AlwaysOnTop)
            {
                // even if we combine multiple monitors,
                // in order to have any of the source monitors on top, we 
                // need to set the combined monitor on top
                newMonitor.AlwaysOnTop = true;
            }
        }

        private void CheckBounds(HeliosVisual visual, Monitor monitor)
        {
            if (visual.DisplayRectangle.Right > monitor.Width)
            {
                visual.Left = Math.Max(0d, monitor.Width - visual.DisplayRectangle.Width);
            }

            if (visual.DisplayRectangle.Bottom > monitor.Height)
            {
                visual.Top = Math.Max(0d, monitor.Height - visual.DisplayRectangle.Height);
            }
        }

        private void ScaleControl(HeliosVisual visual, double scale)
        {
            if (visual.Left > 0)
            {
                double locXDif = visual.Left;
                visual.Left += (locXDif * scale) - locXDif;
            }

            if (visual.Top > 0)
            {
                double locYDif = visual.Top;
                visual.Top += (locYDif * scale) - locYDif;
            }

            visual.Width = Math.Max(visual.Width * scale, 1d);
            visual.Height = Math.Max(visual.Height * scale, 1d);

            visual.ScaleChildren(scale, scale);
        }
    }
}
