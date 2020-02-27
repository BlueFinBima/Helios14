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
    using GadrocsWorkshop.Helios.Controls;
    using System;

    public class ProfileExplorerTreeItem : NotificationObject
    {
        private WeakReference _parent;

        private ProfileExplorerTreeItemType _includeTypes;

        private string _name;
        private string _description;

        private object _item;
        private ProfileExplorerTreeItemType _itemType;
        private ProfileExplorerTreeItemCollection _children;

        private bool _isSelected;
        private bool _isExpanded;

        public ProfileExplorerTreeItem(HeliosProfile profile, ProfileExplorerTreeItemType includeTypes)
            : this(profile.Name, "", null, includeTypes)
        {
            _item = profile;
            _itemType = ProfileExplorerTreeItemType.Profile;

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Monitor))
            {
                ProfileExplorerTreeItem monitors = new ProfileExplorerTreeItem("Monitors", profile.Monitors, this, includeTypes);
                if (monitors.HasChildren)
                {
                    Children.Add(monitors);
                }
            }

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Interface))
            {
                ProfileExplorerTreeItem interfaces = new ProfileExplorerTreeItem("Interfaces", profile.Interfaces, this, includeTypes);
                if (interfaces.HasChildren)
                {
                    Children.Add(interfaces);
                }
                profile.Interfaces.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Interfaces_CollectionChanged);
            }
        }

        void Interfaces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            bool newFolder = false;
            ProfileExplorerTreeItem interfaces;
            if (HasFolder("Interfaces"))
            {
                interfaces = GetFolder("Interfaces");
            }
            else
            {
                interfaces = new ProfileExplorerTreeItem("Interfaces", "", this, _includeTypes);
                newFolder = true;
            }

            if (e.OldItems != null)
            {
                foreach (HeliosInterface heliosInterface in e.OldItems)
                {
                    ProfileExplorerTreeItem child = interfaces.GetChildObject(heliosInterface);
                    if (child != null)
                    {
                        child.Disconnect();
                        interfaces.Children.Remove(child);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosInterface heliosInterface in e.NewItems)
                {
                    ProfileExplorerTreeItem childItem = new ProfileExplorerTreeItem(heliosInterface, this, _includeTypes);
                    interfaces.Children.Add(childItem);
                }
            }

            if (newFolder && interfaces.HasChildren)
            {
                Children.Add(interfaces);
            }
        }

        public ProfileExplorerTreeItem(HeliosObject hobj, ProfileExplorerTreeItemType includeTypes)
            : this(hobj.Name, "", null, includeTypes)
        {
            _item = hobj;
            _itemType = ProfileExplorerTreeItemType.Visual;

            AddChild(hobj, includeTypes);
        }

        private ProfileExplorerTreeItem(string name, string description, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
        {
            _parent = new WeakReference(parent);
            _name = name;
            _description = description;
            _itemType = ProfileExplorerTreeItemType.Folder;
            _includeTypes = includeTypes;
            _children = new ProfileExplorerTreeItemCollection();
        }

        private ProfileExplorerTreeItem(string name, HeliosInterfaceCollection interfaces, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(name, "", parent, includeTypes)
        {
            _itemType = ProfileExplorerTreeItemType.Folder;
            foreach (HeliosInterface heliosInterface in interfaces)
            {
                ProfileExplorerTreeItem item = new ProfileExplorerTreeItem(heliosInterface, this, includeTypes);
                Children.Add(item);
            }
        }

        private ProfileExplorerTreeItem(HeliosInterface heliosInterface, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(heliosInterface.Name, "", parent, includeTypes)
        {
            _itemType = ProfileExplorerTreeItemType.Interface;
            _item = heliosInterface;

            AddChild(heliosInterface, includeTypes);
        }

        private ProfileExplorerTreeItem(string name, MonitorCollection monitors, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(name, "", parent, includeTypes)
        {
            _itemType = ProfileExplorerTreeItemType.Folder;
            foreach (Monitor monitor in monitors)
            {
                ProfileExplorerTreeItem monitorItem = new ProfileExplorerTreeItem(monitor, this, includeTypes);
                Children.Add(monitorItem);
            }
        }

        private ProfileExplorerTreeItem(HeliosVisual visual, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(visual.Name, "", parent, includeTypes)
        {
            if (visual.GetType() == typeof(Monitor))
            {
                _itemType = ProfileExplorerTreeItemType.Monitor;
            }
            else if (visual.GetType() == typeof(HeliosPanel))
            {
                _itemType = ProfileExplorerTreeItemType.Panel;
            }
            else
            {
                _itemType = ProfileExplorerTreeItemType.Visual;
            }
            _item = visual;

            AddChild(visual, includeTypes);

            foreach (HeliosVisual child in visual.Children)
            {
                if ((child is HeliosPanel && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Panel)) ||
                    (child is HeliosVisual && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Visual)))
                {
                    Children.Add(new ProfileExplorerTreeItem(child, this, _includeTypes));
                }
            }

            visual.Children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(VisualChildren_CollectionChanged);
        }

        public void Disconnect()
        {
            switch (ItemType)
            {
                case ProfileExplorerTreeItemType.Monitor:
                case ProfileExplorerTreeItemType.Panel:
                case ProfileExplorerTreeItemType.Visual:
                    HeliosVisual visual = ContextItem as HeliosVisual;
                    visual.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(hobj_PropertyChanged);
                    visual.Children.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(VisualChildren_CollectionChanged);
                    visual.Triggers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Triggers_CollectionChanged);
                    visual.Actions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Actions_CollectionChanged);
                    break;
                case ProfileExplorerTreeItemType.Interface:
                    HeliosInterface item = ContextItem as HeliosInterface;
                    item.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(hobj_PropertyChanged);
                    item.Triggers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Triggers_CollectionChanged);
                    item.Actions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Actions_CollectionChanged);
                    break;
                case ProfileExplorerTreeItemType.Action:
                    IBindingAction action = ContextItem as IBindingAction;
                    action.Target.InputBindings.CollectionChanged -= Bindings_CollectionChanged; 
                    break;
                case ProfileExplorerTreeItemType.Trigger:
                    IBindingTrigger trigger = ContextItem as IBindingTrigger;
                    trigger.Source.OutputBindings.CollectionChanged -= Bindings_CollectionChanged;
                    break;
                case ProfileExplorerTreeItemType.Value:
                    break;
                case ProfileExplorerTreeItemType.Binding:
                    HeliosBinding binding = ContextItem as HeliosBinding;
                    binding.PropertyChanged += Binding_PropertyChanged;
                    break;
                case ProfileExplorerTreeItemType.Profile:
                    HeliosProfile profile = ContextItem as HeliosProfile;
                    profile.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(hobj_PropertyChanged);
                    break;
                default:
                    break;
            }

            foreach (ProfileExplorerTreeItem child in Children)
            {
                child.Disconnect();
            }
        }

        void VisualChildren_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach(HeliosVisual visual in e.OldItems)
                {
                    ProfileExplorerTreeItem child = GetChildObject(visual);
                    if (child != null)
                    {
                        child.Disconnect();
                        Children.Remove(child);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosVisual child in e.NewItems)
                {
                    if ((child is HeliosPanel && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Panel)) ||
                        (child is HeliosVisual && _includeTypes.HasFlag(ProfileExplorerTreeItemType.Visual)))
                    {
                        Children.Add(new ProfileExplorerTreeItem(child, this, _includeTypes));
                    }
                }
            }
        }

        private ProfileExplorerTreeItem(IBindingAction item, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(item.ActionName, item.ActionDescription, parent, includeTypes)
        {
            _item = item;
            _itemType = ProfileExplorerTreeItemType.Action;

            //SortName = item.Name + " " + item.ActionVerb;

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Binding))
            {

                foreach (HeliosBinding binding in item.Owner.InputBindings)
                {
                    if (binding.Action == item)
                    {
                        Children.Add(new ProfileExplorerTreeItem(binding, this, includeTypes));
                    }
                }
                item.Target.InputBindings.CollectionChanged += Bindings_CollectionChanged;                
            }
        }

        void Bindings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (HeliosBinding binding in e.OldItems)
                {
                    ProfileExplorerTreeItem child = GetChildObject(binding);
                    if (child != null)
                    {
                        child.Disconnect();
                        Children.Remove(child);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosBinding child in e.NewItems)
                {
                    if (child.Action == ContextItem || child.Trigger == ContextItem)
                    {
                        ProfileExplorerTreeItem childItem = new ProfileExplorerTreeItem(child, this, _includeTypes);
                        if (_includeTypes.HasFlag(ProfileExplorerTreeItemType.Binding))
                        {
                            IsExpanded = true;
                            Children.Add(childItem);
                        }
                    }
                }
            }
        }

        private ProfileExplorerTreeItem(IBindingTrigger item, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(item.TriggerName, item.TriggerDescription, parent, includeTypes)
        {
            _item = item;
            _itemType = ProfileExplorerTreeItemType.Trigger;

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Binding))
            {
                foreach (HeliosBinding binding in item.Owner.OutputBindings)
                {
                    if (binding.Trigger == item)
                    {
                        Children.Add(new ProfileExplorerTreeItem(binding, this, includeTypes));
                    }
                }
                item.Source.OutputBindings.CollectionChanged += Bindings_CollectionChanged;
            }
        }

        private ProfileExplorerTreeItem(HeliosBinding item, ProfileExplorerTreeItem parent, ProfileExplorerTreeItemType includeTypes)
            : this(item.Description, "", parent, includeTypes)
        {
            _item = item;
            _itemType = ProfileExplorerTreeItemType.Binding;
            item.PropertyChanged += Binding_PropertyChanged;
        }

        void Binding_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Name"))
            {
                Name = (_item as HeliosBinding).Description;
            }
        }

        public void ExpandAll()
        {
            IsExpanded = true;
            foreach (ProfileExplorerTreeItem child in Children)
            {
                child.ExpandAll();
            }
        }

        #region Properties

        public bool HasChildren { get { return _children != null && _children.Count > 0; } }

        public ProfileExplorerTreeItem Parent
        {
            get { return _parent.Target as ProfileExplorerTreeItem; }
            private set { _parent = new WeakReference(value); }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if ((_name == null && value != null)
                    || (_name != null && !_name.Equals(value)))
                {
                    string oldValue = _name;
                    _name = value;
                    OnPropertyChanged("Name", oldValue, value, false);
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (!_isSelected.Equals(value))
                {
                    bool oldValue = _isSelected;
                    _isSelected = value;
                    OnPropertyChanged("IsSelected", oldValue, value, false);
                }
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (!_isExpanded.Equals(value))
                {
                    bool oldValue = _isExpanded;
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded", oldValue, value, false);
                }
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if ((_description == null && value != null)
                    || (_description != null && !_description.Equals(value)))
                {
                    string oldValue = _description;
                    _description = value;
                    OnPropertyChanged("Description", oldValue, value, false);
                }
            }
        }

        public ProfileExplorerTreeItemType ItemType
        {
            get { return _itemType; }
        }

        public ProfileExplorerTreeItemCollection Children
        {
            get { return _children; }
        }

        public object ContextItem
        {
            get { return _item; }
        }
                
        #endregion

        private void AddChild(HeliosObject hobj, ProfileExplorerTreeItemType includeTypes)
        {
            hobj.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(hobj_PropertyChanged);

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Trigger))
            {
                foreach (IBindingTrigger trigger in hobj.Triggers)
                {
                    AddTrigger(trigger, includeTypes);
                }
                hobj.Triggers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Triggers_CollectionChanged);
            }

            if (includeTypes.HasFlag(ProfileExplorerTreeItemType.Action))
            {
                foreach (IBindingAction action in hobj.Actions)
                {
                    AddAction(action, includeTypes);
                }
                hobj.Actions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Actions_CollectionChanged);
            }
        }

        void hobj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HeliosObject obj = _item as HeliosObject;
            if (e.PropertyName.Equals("Name") && obj != null)
            {
                this.Name = obj.Name;
            }
        }

        void Triggers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IBindingTrigger trigger in e.OldItems)
                {
                    if (trigger.Device.Length > 0)
                    {
                        ProfileExplorerTreeItem folder = GetFolder(trigger.Device);
                        folder.Children.Remove(folder.GetChildObject(trigger));
                        if (folder.Children.Count == 0)
                        {
                            Children.Remove(folder);
                        }
                    }
                    else
                    {
                        Children.Remove(GetChildObject(trigger));
                    }

                }
            }

            if (e.NewItems != null)
            {
                foreach (IBindingTrigger trigger in e.NewItems)
                {
                    AddTrigger(trigger, _includeTypes);
                }
            }

        }

        void Actions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IBindingAction action in e.OldItems)
                {
                    if (action.Device.Length > 0)
                    {
                        ProfileExplorerTreeItem folder = GetFolder(action.Device);
                        folder.Children.Remove(folder.GetChildObject(action));
                        if (folder.Children.Count == 0)
                        {
                            Children.Remove(folder);
                        }
                    }
                    else
                    {
                        Children.Remove(GetChildObject(action));
                    }

                }
            }

            if (e.NewItems != null)
            {
                foreach (IBindingAction action in e.NewItems)
                {
                    AddAction(action, _includeTypes);
                }
            }
        }

        private void AddTrigger(IBindingTrigger trigger, ProfileExplorerTreeItemType includeTypes)
        {

            ProfileExplorerTreeItem triggerItem = new ProfileExplorerTreeItem(trigger, this, includeTypes);
            if (triggerItem.HasChildren || includeTypes.HasFlag(ProfileExplorerTreeItemType.Trigger))
            {
                if (trigger.Device.Length > 0)
                {
                    if (!HasFolder(trigger.Device))
                    {
                        Children.Add(new ProfileExplorerTreeItem(trigger.Device, "", this, includeTypes));
                    }

                    ProfileExplorerTreeItem deviceItem = GetFolder(trigger.Device);
                    triggerItem.Parent = deviceItem;
                    deviceItem.Children.Add(triggerItem);
                }
                else
                {
                    Children.Add(triggerItem);
                }
            }
        }

        public void AddAction(IBindingAction action, ProfileExplorerTreeItemType includeTypes)
        {
            ProfileExplorerTreeItem actionItem = new ProfileExplorerTreeItem(action, this, includeTypes);
            if (actionItem.HasChildren || includeTypes.HasFlag(ProfileExplorerTreeItemType.Action))
            {
                if (action.Device.Length > 0)
                {
                    if (!HasFolder(action.Device))
                    {
                        Children.Add(new ProfileExplorerTreeItem(action.Device, "", this, includeTypes));
                    }

                    ProfileExplorerTreeItem deviceItem = GetFolder(action.Device);
                    actionItem.Parent = deviceItem;
                    deviceItem.Children.Add(actionItem);
                }
                else
                {
                    Children.Add(actionItem);
                }
            }
        }

        private bool HasFolder(string folderName)
        {
            return GetFolder(folderName) != null;
        }

        private ProfileExplorerTreeItem GetFolder(string folderName)
        {
            foreach (ProfileExplorerTreeItem child in Children)
            {
                if (child.Name.Equals(folderName) && child.ItemType == ProfileExplorerTreeItemType.Folder)
                {
                    return child;
                }
            }
            return null;
        }

        private bool HasChildObject(HeliosObject childObject)
        {
            return GetChildObject(childObject) != null;
        }

        private ProfileExplorerTreeItem GetChildObject(object childObject)
        {
            foreach (ProfileExplorerTreeItem child in Children)
            {
                if (child.ContextItem == childObject)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
