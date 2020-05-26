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
// 

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ShadowViewportEventArgs : EventArgs
    {
        public ShadowVisual Data { get; }

        public ShadowViewportEventArgs(ShadowVisual shadow)
        {
            Data = shadow;
        }
    }

    public class RawViewportEventArgs : EventArgs
    {
        public HeliosVisual Raw { get; }

        public RawViewportEventArgs(HeliosVisual visual)
        {
            Raw = visual;
        }
    }

    /// <summary>
    /// callbacks from objects shadowing visuals (viewports, monitors) in the Helios Profile
    /// to implement our model
    /// </summary>
    public interface IShadowVisualParent
    {
        Vector GlobalOffset { get; }
        double Scale { get; }
        void AddViewport(ShadowVisual viewport);
        void RemoveViewport(ShadowVisual viewport);
    }

    /// <summary>
    /// This class represents a Helios visual being observed.
    /// It is the model class.
    /// </summary>
    public class ShadowVisual : NotificationObject
    {
        public event EventHandler<RawViewportEventArgs> ViewportChanged;
        public event EventHandler<RawMonitorEventArgs> MonitorChanged;

        /// <summary>
        /// callbacks to our parent
        /// </summary>
        protected IShadowVisualParent _parent;

        /// <summary>
        /// the visual we are shadowing
        /// </summary>
        protected HeliosVisual _visual;

        /// <summary>
        /// map from visual child to its shadow representation
        /// </summary>
        protected Dictionary<HeliosVisual, ShadowVisual> _children = new Dictionary<HeliosVisual, ShadowVisual>();

        /// <summary>
        /// viewport extent or null if this item is not a viewport
        /// </summary>
        protected IViewportExtent _viewport;

        /// <summary>
        /// the monitor on which this visual is placed
        /// </summary>
        protected Monitor _monitor;

        public bool IsViewport => _viewport != null;

        public Dictionary<HeliosVisual, ShadowVisual> Children => _children;

        public HeliosVisual Visual => _visual;

        public Monitor Monitor => _monitor;

        public IViewportExtent Viewport => _viewport;

        internal ShadowVisual(IShadowVisualParent parent, Monitor monitor, HeliosVisual visual, bool recurse = true)
        {
            // create a shadow object
            _parent = parent;
            _monitor = monitor;
            _visual = visual;
            _viewport = visual as IViewportExtent;
            if (IsViewport)
            {
                _parent.AddViewport(this);
            }

            // observe changes
            monitor.Moved += Monitor_Modified;
            monitor.Resized += Monitor_Modified;
            visual.Moved += Visual_Modified;
            visual.Resized += Visual_Modified;
            visual.Children.CollectionChanged += Visual_Children_CollectionChanged;

            // NOTE: changes in monitor children tracked in ShadowMonitor

            if (recurse)
            {
                Instrument(monitor, visual);
            }
        }

        private void Monitor_Modified(object sender, EventArgs e)
        {
            MonitorChanged?.Invoke(this, new RawMonitorEventArgs(_monitor));
        }

        private void Visual_Modified(object sender, EventArgs e)
        {
            OnVisualModified();
        }

        /// <summary>
        /// recusively add all descendants for tracking
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="visual"></param>
        protected void Instrument(Monitor monitor, HeliosVisual visual)
        {
            foreach (HeliosVisual child in visual.Children)
            {
                _children[child] = new ShadowVisual(_parent, monitor, child);
            }
        }

        private void Visual_Children_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (HeliosVisual newChild in e.NewItems)
                {
                    OnAdded(newChild);
                }
            }

            if (e.OldItems != null)
            {
                foreach (HeliosVisual oldChild in e.OldItems)
                {
                    OnRemoved(oldChild);
                }
            }
        }

        protected virtual void OnAdded(HeliosVisual newChild)
        {
            _children[newChild] = new ShadowVisual(_parent, _monitor, newChild);
        }

        protected virtual void OnRemoved(HeliosVisual oldChild)
        {
            if (!_children.ContainsKey(oldChild))
            {
                // this has happened before and it is unclear why
                ConfigManager.LogManager.LogInfo(
                    $"the Visual object {oldChild.Name} of type {oldChild.TypeIdentifier} was not found in the data used for tracking viewports and monitors; probable program error");
                return;
            }

            ShadowVisual shadow = _children[oldChild];
            shadow.Dispose();
            _children.Remove(oldChild);
        }

        protected virtual void OnVisualModified()
        {
            if (!IsViewport)
            {
                return;
            }

            // we actually care about this change
            ViewportChanged?.Invoke(this, new RawViewportEventArgs(_visual));
        }

        internal virtual void Dispose()
        {
            if (IsViewport)
            {
                _parent.RemoveViewport(this);
            }

            _visual.Moved -= Visual_Modified;
            _visual.Resized -= Visual_Modified;
            _visual.Children.CollectionChanged -= Visual_Children_CollectionChanged;
            foreach (ShadowVisual child in _children.Values)
            {
                child.Dispose();
            }
        }
    }
}