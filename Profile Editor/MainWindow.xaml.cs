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

using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Interfaces.Common;
using GadrocsWorkshop.Helios.ProfileEditor.ViewModel;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using GadrocsWorkshop.Helios.ProfileEditor.UndoEvents;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using GadrocsWorkshop.Helios.Windows.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using AvalonDock.Layout;
    using AvalonDock.Layout.Serialization;

    /// <summary>
    /// we may refactor profile loading into here if we can untangle it enough
    /// for now, it just hosts an extra logger with a civilized name
    /// </summary>
    internal class ProfileLoader
    {
        internal static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ResetMonitorsWork.ICallbacks
    { 
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Internal Class used to track open documents
        /// </summary>
        private class DocumentMeta
        {
            public HeliosObject hobj;
            public LayoutDocument document;
            public HeliosEditorDocument editor;
        }

        private delegate HeliosProfile LoadProfileDelegate(string filename);
        private delegate void LayoutDelegate(string filename);

        private readonly XmlLayoutSerializer _layoutSerializer;
        private string _systemDefaultLayout;
        private readonly string _defaultLayoutFile;

        private LoadingAdorner _loadingAdorner;
        private readonly List<DocumentMeta> _documents = new List<DocumentMeta>();

        private bool _initialLoad = true;

        private readonly InterfaceStatusScanner _configurationCheck = new InterfaceStatusScanner();

        public MainWindow()
        {
            InitializeComponent();

            InterfaceStatusPanel.DataContext = new InterfaceStatusViewModel(_configurationCheck);
            DockManager.ActiveContentChanged += DockManager_ActiveDocumentChanged;
            NewProfile();

            _layoutSerializer = new XmlLayoutSerializer(DockManager);
            _layoutSerializer.LayoutSerializationCallback += LayoutSerializer_LayoutSerializationCallback;

            _defaultLayoutFile = Path.Combine(ConfigManager.DocumentPath, "DefaultLayout.hply");

            // arm this to tell us if any interface reports status above a configured threshold
            _configurationCheck.Triggered += ConfigurationCheck_Triggered;
        }

        void LayoutSerializer_LayoutSerializationCallback(object sender, LayoutSerializationCallbackEventArgs e)
        {            
            if (Profile != null && e.Model is LayoutDocument)
            {
                HeliosObject profileObject = HeliosSerializer.ResolveReferenceName(Profile, e.Model.ContentId);
                if (profileObject != null)
                {
                    HeliosEditorDocument editor = CreateDocumentEditor(profileObject);
                    if (editor == null)
                    {
                        // this component used to have an editor and now it does not, or more likely this is
                        // an UnsupportedInterface and therefore has no editor
                        Logger.Debug("Layout Serializer: Document {ContentId} does not have an editor; ignored", e.Model.ContentId);
                        return;
                    }
                    profileObject.PropertyChanged += DocumentObject_PropertyChanged;
                    e.Content = CreateDocumentContent(editor);
                    //DocumentPane.Children.Add((LayoutDocument)e.Model);
                    e.Model.Closed += Document_Closed;
                    AddDocumentMeta(profileObject, (LayoutDocument)e.Model, editor);
                } else
                {
                    Logger.Debug("Layout Serializer: Unable to resolve Layout Document " + e.Model.ContentId);
                }
            }
        }

        #region Properties

        public HeliosProfile Profile
        {
            get { return (HeliosProfile)GetValue(ProfileProperty); }
            set
            {
                Profile?.Unload();
                if (Profile != null)
                {
                    // dont' show stale sources and bindings
                    BindingsPanel.Clear();
                }
                SetValue(ProfileProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Profile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(HeliosProfile), typeof(MainWindow), new PropertyMetadata(null));

        public int Monitor
        {
            get { return (int)GetValue(MonitorProperty); }
            set { SetValue(MonitorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Monitor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MonitorProperty =
            DependencyProperty.Register("Monitor", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public string StatusBarMessage
        {
            get { return (string)GetValue(StatusBarMessageProperty); }
            set { SetValue(StatusBarMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarMessageProperty =
            DependencyProperty.Register("StatusBarMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public HeliosEditorDocument CurrentEditor
        {
            get { return (HeliosEditorDocument)GetValue(CurrentEditorProperty); }
            set { SetValue(CurrentEditorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentEditor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentEditorProperty =
            DependencyProperty.Register("CurrentEditor", typeof(HeliosEditorDocument), typeof(MainWindow), new PropertyMetadata(null));

        /// <summary>
        /// true if Closing event has been raised
        /// </summary>
        private bool _bClosing;

        #endregion

        #region Event Handlers

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_initialLoad)
            {
                _initialLoad = false;
                Helios.ProfileEditor.App app = Application.Current as Helios.ProfileEditor.App;
                if (app != null && app.StartupProfile != null && System.IO.File.Exists(app.StartupProfile))
                {
                    LoadProfile(app.StartupProfile);
                }
            }
        }

        void DockManager_ActiveDocumentChanged(object sender, EventArgs e)
        {
            HeliosEditorDocument activeDocument = null;

            // Interface Editor documents are embeded in a scrollviewer.  Unwrap them if they are
            // the current content.
            ScrollViewer viewer = DockManager.ActiveContent as ScrollViewer;
            if (viewer != null)
            {
                activeDocument = viewer.Content as HeliosEditorDocument;
            }
            else
            {
                activeDocument = DockManager.ActiveContent as HeliosEditorDocument;
            }            
            if (activeDocument != null)
            {
                if (activeDocument != CurrentEditor)
                {
                    CurrentEditor = activeDocument;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ProfileProperty)
            {
                HeliosProfile oldProfile = e.OldValue as HeliosProfile;
                if (oldProfile != null)
                {
                    oldProfile.PropertyChanged -= new PropertyChangedEventHandler(Profile_PropertyChanged);
                }

                foreach (DocumentMeta meta in _documents.ToArray())
                {
                    meta.document.Close();
                    meta.hobj.PropertyChanged -= DocumentObject_PropertyChanged;
                }
                _documents.Clear();

                if (Profile != null)
                {
                    Profile.PropertyChanged += new PropertyChangedEventHandler(Profile_PropertyChanged);
                }
            }
            base.OnPropertyChanged(e);
        }

        void Profile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyNotificationEventArgs args = e as PropertyNotificationEventArgs;
            if (args != null)
            {
                ConfigManager.UndoManager.AddPropertyChange(sender, args);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CheckSave())
            {
                e.Cancel = true;
            }

            if (e.Cancel == false)
            {
                Logger.Debug("Profile Editor main window has raised OnClosing and is going down");
                _bClosing = true;
                ConfigManager.UndoManager.ClearHistory();

                // Persist window placement details to application settings
                NativeMethods.WINDOWPLACEMENT wp = new NativeMethods.WINDOWPLACEMENT();
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.GetWindowPlacement(hwnd, out wp);

                ConfigManager.SettingsManager.SaveSetting("ProfileEditor", "WindowLocation", wp.normalPosition);

                // Close all open documents so they can clean up
                while (_documents.Count > 0)
                {
                    _documents[0].document.Close();
                }

                // unload profile if present
                Profile = null;
            }
            base.OnClosing(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Load window placement details for previous application session from application settings
            // Note - if window was closed on a monitor that is now disconnected from the computer,
            //        SetWindowPlacement will place the window onto a visible monitor.

            if (ConfigManager.SettingsManager.IsSettingAvailable("ProfileEditor", "WindowLocation"))
            {
                NativeMethods.WINDOWPLACEMENT wp = new NativeMethods.WINDOWPLACEMENT();
                wp.normalPosition = ConfigManager.SettingsManager.LoadSetting("ProfileEditor", "WindowLocation", new NativeMethods.RECT(0, 0, (int)Width, (int)Height));
                wp.length = Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = (wp.showCmd == NativeMethods.SW_SHOWMINIMIZED ? NativeMethods.SW_SHOWNORMAL : wp.showCmd);
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.SetWindowPlacement(hwnd, ref wp);
            }

            base.OnSourceInitialized(e);
        }

        #endregion

        #region Adorners

        private LoadingAdorner GetLoadingAdorner()
        {
            if (_loadingAdorner == null)
            {
                _loadingAdorner = new LoadingAdorner(PrimaryGrid);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(PrimaryGrid);
                layer.Add(_loadingAdorner);
            }
            return _loadingAdorner;
        }

        private void RemoveLoadingAdorner()
        {
            if (_loadingAdorner != null)
            {
                _loadingAdorner.Visibility = Visibility.Hidden;
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(PrimaryGrid);
                layer.Remove(_loadingAdorner);
                _loadingAdorner = null;
            }
        }

        #endregion

        #region Helper Methods

        private DocumentMeta AddDocumentMeta(HeliosObject profileObject, LayoutDocument document, HeliosEditorDocument editor)
        {
            DocumentMeta meta = new DocumentMeta {editor = editor, document = document, hobj = profileObject};
            _documents.Add(meta);
            return meta;
        }

        private DocumentMeta FindDocumentMeta(HeliosObject profileObject)
        {
            return _documents.FirstOrDefault(meta => meta.hobj == profileObject);
        }

        private DocumentMeta FindDocumentMeta(LayoutDocument document)
        {
            return _documents.FirstOrDefault(meta => meta.document == document);
        }

        private DocumentMeta FindDocumentMeta(HeliosEditorDocument editor)
        {
            return _documents.FirstOrDefault(meta => meta.editor == editor);
        }

        private DocumentMeta AddNewDocument(HeliosObject profileObject)
        {
            DocumentMeta meta = FindDocumentMeta(profileObject);
            if (meta != null)
            {
                meta.document.IsSelected = true;
                return meta;
            }


            HeliosEditorDocument editor = CreateDocumentEditor(profileObject);
            if (editor == null)
            {
                return null;
            }

            LayoutDocument document = new LayoutDocument
            {
                Title = editor.Title,
                IsSelected = true,
                ContentId = HeliosSerializer.GetReferenceName(profileObject),
                Content = CreateDocumentContent(editor)
            };

            // Since a new LayoutRoot object is created upon de-serialization, the Child LayoutDocumentPane no longer belongs to the LayoutRoot 
            // therefore the LayoutDocumentPane 'DocumentPane' must be referred to dynamically
            LayoutDocumentPane documentPane = DockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            documentPane?.Children.Add(document);
            document.IsSelected = true;
            document.Closed += Document_Closed;

            meta = AddDocumentMeta(profileObject, document, editor);
            profileObject.PropertyChanged += DocumentObject_PropertyChanged;

            return meta;
        }
        
        private HeliosEditorDocument CreateDocumentEditor(HeliosObject profileObject)
        {
            switch (profileObject)
            {
                case Monitor monitor:
                    return new MonitorDocument(monitor);
                case HeliosPanel panel:
                    return new PanelDocument(panel);
                case HeliosInterface heliosInterface:
                {
                    HeliosEditorDocument editor = ConfigManager.ModuleManager.CreateInterfaceEditor(heliosInterface, Profile);
                    if (editor != null)
                    {
                        editor.Style = Application.Current.Resources["InterfaceEditor"] as Style;
                    }
                    return editor;
                }
                default:
                    throw new ArgumentException(@"Cannot create a editor document for profileobject requested.", nameof(profileObject));
            }
        }

        private object CreateDocumentContent(HeliosEditorDocument editor)
        {
            if (editor.HandlesScroll)
            {
                return editor;
            }

            ScrollViewer scroller = new ScrollViewer
            {
                Content = editor,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            return scroller;
        }

        void Document_Closed(object sender, EventArgs e)
        {
            DocumentMeta meta = FindDocumentMeta(sender as LayoutDocument);

            if (meta == null)
            {
                throw new InvalidOperationException("Document closed called for a document not found in meta data.");
            }

            meta.editor.Closed();
            meta.hobj.PropertyChanged -= DocumentObject_PropertyChanged;
            meta.document.Closed -= Document_Closed;

            _documents.Remove(meta);
        }

        void DocumentObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DocumentMeta meta = FindDocumentMeta(sender as HeliosObject);
            if (meta == null)
            {
                throw new InvalidOperationException("Property Changed closed called for a profile object not found in meta data.");
            }

            if (e.PropertyName.Equals("Name"))
            {
                meta.document.Title = meta.editor.Title;
                meta.document.ContentId = "doc:" + HeliosSerializer.GetReferenceName(meta.hobj);
            }
        }

        #endregion

        #region Profile Persistance

        private void NewProfile()
        {
            if (CheckSave())
            {
                Profile = new HeliosProfile();
                ConfigManager.UndoManager.ClearHistory();

                AddNewDocument(Profile.Monitors[0]);
                OnProfileChangeComplete();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private bool CheckSave()
        {
            if (!ConfigManager.UndoManager.CanUndo && (Profile == null || !Profile.IsDirty))
            {
                return true;
            }

            MessageBoxResult result = MessageBox.Show(this, "There are changes to the current profile.  If you continue without saving your changes, they will be lost.  Would you like to save the current profile?", "Save Changes", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                return SaveProfile();
            }
            return (result != MessageBoxResult.Cancel);
        }

        private void OpenProfile()
        {
            if (!CheckSave())
            {
                return;
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = Profile.Name; // Default file name
            dlg.DefaultExt = ".hpf"; // Default file extension
            dlg.Filter = "Helios Profiles (.hpf)|*.hpf"; // Filter files by extension
            dlg.InitialDirectory = ConfigManager.ProfilePath;
            dlg.ValidateNames = true;
            dlg.AddExtension = true;
            dlg.Multiselect = false;
            dlg.Title = "Open Profile";

            // Show open file dialog box
            bool? result = dlg.ShowDialog(this);

            // Process open file dialog box results
            if (result == true)
            {
                LoadProfile(dlg.FileName);
            }
        }

        private void LoadProfile(string path)
        {
            StatusBarMessage = "Loading Profile...";
            GetLoadingAdorner();

            // create profile
            string systemLayoutText = _systemDefaultLayout;
            HeliosProfile profile = ConfigManager.ProfileManager.LoadProfile(path, out IEnumerable<string> loadingWork);

            // now load profile components in little chunks, because it may take a while
            foreach (string progress in loadingWork)
            {
                if (!PumpUserInterface())
                {
                    return;
                }
                 
                // log progress
                ProfileLoader.Logger.Debug(progress);
            }

            // everything is considered clean now
            ConfigManager.UndoManager.ClearHistory();

            // Load the graphics so everything is more responsive after load
            if (profile != null)
            {
                foreach (Monitor monitor in profile.Monitors)
                {
                    LoadVisual(monitor);
                    if (!PumpUserInterface())
                    {
                        return;
                    }
                }

                // NOTE: only install profile if not null (i.e. load succeeded)
                Profile = profile;

                string layoutFileName = Path.ChangeExtension(profile.Path, "hply");
                if (File.Exists(layoutFileName))
                {
                    if (LayoutIsComplete(systemLayoutText, layoutFileName))
                    {
                        _layoutSerializer.Deserialize(layoutFileName);
                    }
                }
            } 
            else
            {
                Logger.Error($"Failed to load profile '{path}'; continuing with previously loaded profile");
                MessageBox.Show(
                    "Profile Editor will continuing with the previously loaded profile.  Please inspect the Profile Editor logs and file a bug if appropriate.",
                    $"Failed to load profile '{path}'",
                    MessageBoxButton.OK);
            }

            RemoveLoadingAdorner();
            SetValue(StatusBarMessageProperty, "");
            OnProfileChangeComplete();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        /// <summary>
        /// pump the UI and abort if the our window is closing or the Dispatcher is going down
        /// </summary>
        /// <returns>false if we are exiting</returns>
        private bool PumpUserInterface()
        {
            // first check if our window is being closed, because we will exit if we pump the UI in that state
            if (_bClosing)
            {
                // Profile Editor main window canceling work because window is closing
                // NOTE: logger is dead at this point
                return false;
            }

            // now pump the UI or find out that the Dispatcher is null or dead
            bool bRunning = false;
            Dispatcher?.Invoke(() => { bRunning = true; }, DispatcherPriority.ContextIdle);
            if (_bClosing || !bRunning)
            {
                // Profile Editor main window canceling work because window is closing or application is shut down
                // NOTE: logger is dead at this point
                return false;
            }

            return true;
        }

        /// <summary>
        /// tries to determine if a given layout file will abandon any controls if loaded, by comparing
        /// to the system default layout that is dynamically generated on every startup
        /// </summary>
        /// <param name="layoutFileName"></param>
        /// <returns></returns>
        private static bool LayoutIsComplete(string systemLayoutText, string layoutFileName)
        {
            Regex contentId = new Regex("ContentId=\"([^\"]+)\"", RegexOptions.Compiled);
            string layout = File.ReadAllText(layoutFileName);
            foreach (Match match in contentId.Matches(systemLayoutText))
            {
                if (match.Groups.Count < 2)
                {
                    continue;
                }
                if (!match.Groups[1].Success)
                {
                    continue;
                }
                if (match.Groups[1].Value.StartsWith("Visual;"))
                {
                    // ignore monitor that is added by default
                    continue;
                }
                if (!layout.Contains(match.Groups[0].Value))
                {
                    // there is a named panel in this version of the software that
                    // would be removed if we load this layout
                    Logger.Info($"ignoring saved editor layout because it does not specify a location for the required control '{match.Groups[0].Value}'");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// called when creating or loading a profile is complete
        /// </summary>
        private void OnProfileChangeComplete()
        { 
            _configurationCheck?.Reload(Profile);
        }

        private void ConfigurationCheck_Triggered(object sender, EventArgs e)
        {
            // REVISIT: this is expensive.  add tracking whether it is currently closed or hidden, so we don't have to search?
#if true
            _ = ShowCurrentLayoutAnchorable(InterfaceStatusPanel);
#else
            // XXX do this only on profile load if there are issues
            LayoutAnchorable interfaceStatus = ShowCurrentLayoutAnchorable(InterfaceStatusPanel);
            if (interfaceStatus != null)
            {
                interfaceStatus.IsSelected = true;
            }
#endif
        }

        private void LoadVisual(HeliosVisual visual)
        {
            visual.Renderer.Dispatcher = Dispatcher;
            visual.Renderer.Refresh();
            foreach (HeliosVisual control in visual.Children)
            {
                if (!PumpUserInterface())
                {
                    return;
                }
                LoadVisual(control);
            }
        }

        private bool SaveProfile()
        {
            if (string.IsNullOrEmpty(Profile.Path))
            {
                return SaveAsProfile();
            }

            WriteProfile(Profile);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return true;
        }

        private void WriteProfile(HeliosProfile profile)
        {
            StatusBarMessage = "Saving Profile...";
            GetLoadingAdorner();

            System.Threading.Thread t = new System.Threading.Thread(delegate()
            {
                if (!ConfigManager.ProfileManager.SaveProfile(profile))
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)ErrorDuringSave);
                }
                ConfigManager.UndoManager.ClearHistory();
                profile.IsDirty = false;
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(RemoveLoadingAdorner));
                Dispatcher.Invoke(DispatcherPriority.Background, (System.Threading.SendOrPostCallback)delegate { SetValue(StatusBarMessageProperty, ""); }, "");

                string layoutFileName = Path.ChangeExtension(profile.Path, "hply");
                if (File.Exists(layoutFileName))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute file exists asserted above
                    File.Delete(layoutFileName);
                }
                Dispatcher.Invoke(DispatcherPriority.Background, (LayoutDelegate)_layoutSerializer.Serialize, layoutFileName);
            });
            t.Start();
        }

        private void ErrorDuringSave()
        {
            MessageBox.Show(this, "There was an error saving your profile.  Please contact support.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool SaveAsProfile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = Profile.Name; // Default file name
            dlg.DefaultExt = ".hpf"; // Default file extension
            dlg.Filter = "Helios Profiles (.hpf)|*.hpf"; // Filter files by extension
            dlg.InitialDirectory = ConfigManager.ProfilePath;
            dlg.OverwritePrompt = true;
            dlg.ValidateNames = true;
            dlg.AddExtension = true;
            dlg.Title = "Save Profile As";

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                Profile.Path = dlg.FileName;
                Profile.Name = System.IO.Path.GetFileNameWithoutExtension(Profile.Path);
                WriteProfile(Profile);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return true;
            }

            return false;
        }

#endregion

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About dialog = new About {Owner = this};
            dialog.ShowDialog();
        }

        private void NewVersionCheck_Click(object sender, RoutedEventArgs e)
        {
            VersionChecker.CheckVersion();
        }

#region Commands

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewProfile();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenProfile();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveProfile();
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAsProfile();
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void OpenProfileItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DocumentMeta meta = AddNewDocument(e.Parameter as HeliosObject);
            if (meta?.document.Content is HeliosVisualContainerEditor)
            {
                meta.editor.SetBindingFocus((HeliosObject)e.Parameter);
            }
        }

        private void CloseProfileItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseProfileItem(e.Parameter);
        }

        public void CloseProfileItem(object item)
        {
            DocumentMeta meta = FindDocumentMeta(item as HeliosObject);

            if (meta == null)
            {
                return;
            }

            meta.document.Close();
            meta.hobj.PropertyChanged -= DocumentObject_PropertyChanged;
        }

        public void OnExecuteUndo(object sender, ExecutedRoutedEventArgs e)
        {
            ConfigManager.UndoManager.Undo();
        }

        public void OnCanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ConfigManager.UndoManager.CanUndo;
        }

        public void OnExecuteRedo(object sender, ExecutedRoutedEventArgs e)
        {
            ConfigManager.UndoManager.Redo();
        }

        public void OnCanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ConfigManager.UndoManager.CanRedo;
        }

        private void AddInterface_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AvailableInterfaces availableInterfaces = new AvailableInterfaces(Profile);
            AddInterfaceDialog dialog = new AddInterfaceDialog();
            dialog.DataContext = availableInterfaces;
            dialog.Owner = this;

            try
            {
                bool? result = dialog.ShowDialog();

                // shut down async searches
                availableInterfaces.Dispose();
                dialog.DataContext = null;

                // anything selected?
                if (result != true || availableInterfaces.SelectedInterface == null)
                {
                    return;
                }
                
                // retrieve the interface instance, which may be created now or may already be in the item
                HeliosInterface heliosInterface = availableInterfaces.SelectedInterface.HeliosInterface;
                string name = heliosInterface.Name;
                int count = 0;
                while (Profile.Interfaces.ContainsKey(name))
                {
                    name = heliosInterface.Name + " " + ++count;
                }
                heliosInterface.Name = name;

                ConfigManager.UndoManager.AddUndoItem(new InterfaceAddUndoEvent(Profile, heliosInterface));
                Profile.Interfaces.Add(heliosInterface);
                AddNewDocument(heliosInterface);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AddInterface - Error during add Interface dialog or creation");
            }
        }

        private void SaveLayout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_defaultLayoutFile))
            {
                File.Delete(_defaultLayoutFile);
            }
            _layoutSerializer.Serialize(_defaultLayoutFile);
        }

        private void LoadLayout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_defaultLayoutFile))
            {
                _layoutSerializer.Deserialize(_defaultLayoutFile);
            }
            else
            {
                StringReader reader = new StringReader(_systemDefaultLayout);
                _layoutSerializer.Deserialize(reader);
            }
        }

        private void RestoreDefaultLayout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_defaultLayoutFile))
            {
                File.Delete(_defaultLayoutFile);
            }
            // we need to close any editor documents before we deserialize the layout because they will be removed but not closed and
            // then can never be recovered
            CloseAllDocuments();
            StringReader reader = new StringReader(_systemDefaultLayout);
            _layoutSerializer.Deserialize(reader);
        }

        private void DialogShowModal_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // this is the source of the event, and we will resolve DataTemplate from their position
            FrameworkElement host = (FrameworkElement)e.OriginalSource;

            // crash if incorrect parameter type
            ShowModalParameter parameter = (ShowModalParameter) e.Parameter;

            // resolve the data template
            DataTemplate template = parameter.DataTemplate ?? (DataTemplate)host.TryFindResource(new DataTemplateKey(parameter.Content.GetType()));

            // display the dialog appropriate to the content
            Window generic = new DefaultDialogWindow
            {
                ContentTemplate = template, 
                Content = parameter.Content
            };
            
            generic.ShowDialog();
        }

        private void CloseAllDocuments()
        {
            // get as stable snapshot to enumerate
            List<DocumentMeta> documents = new List<DocumentMeta>(_documents);
            foreach (DocumentMeta meta in documents)
            {
                meta.document.Close();
            }
        }

        private void GoThere_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InterfaceStatusViewSection viewSection = (InterfaceStatusViewSection)(e.Parameter);
            if (viewSection.Data.HasEditor)
            {
                AddNewDocument(viewSection.Data.Interface);
            }
        }

        private void DeleteInterface_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // redirect delete received from check list to profile explorer
            HeliosInterface heliosInterface = e.Parameter as HeliosInterface;
            if (heliosInterface != null)
            {
                ExplorerPanel.DeleteInterface(heliosInterface);
            }
        }

#endregion

        private void Show_Preview(object sender, RoutedEventArgs e)
        {
            ShowCurrentLayoutAnchorable(PreviewPanel);
        }

        private void Show_Toolbox(object sender, RoutedEventArgs e)
        {
            ShowCurrentLayoutAnchorable(ToolboxPanel);
        }

        private void Show_InterfaceStatus(object sender, RoutedEventArgs e)
        {
            ShowCurrentLayoutAnchorable(InterfaceStatusPanel);
        }

        /// <summary>
        /// try to find the layout panel that is now hosting the specified control
        /// and display it.  During every layout deserialization, these layout panels
        /// are replaced so we don't keep references to them.
        /// </summary>
        /// <param name="withContent"></param>
        /// <returns>the LayoutAnchorable associated with the content or null</returns>
        private LayoutAnchorable ShowCurrentLayoutAnchorable(object withContent)
        {
            // LayoutAnchorable current = FindLayoutAnchorable(DockManager.Layout, withContent);
            LayoutAnchorable current = DockManager.Layout.Descendents().OfType<LayoutAnchorable>().First(l => l.Content == withContent);
            current?.Show();
            return current;
        }

        private void Show_Explorer(object sender, RoutedEventArgs e)
        {
            _ = ShowCurrentLayoutAnchorable(ExplorerPanel);
        }

        private void Show_Properties(object sender, RoutedEventArgs e)
        {
            _ = ShowCurrentLayoutAnchorable(PropertiesPanel);
        }

        private void Show_Bindings(object sender, RoutedEventArgs e)
        {
            _ = ShowCurrentLayoutAnchorable(BindingsPanel);
        }

        private void Show_Layers(object sender, RoutedEventArgs e)
        {
            _ = ShowCurrentLayoutAnchorable(LayersPanel);
        }

        private void Show_TemplateManager(object sender, RoutedEventArgs e)
        {
            TemplateManagerWindow tm = new TemplateManagerWindow {Owner = this};
            tm.ShowDialog();
        }

        private void ResetMonitors_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResetMonitors resetDialog = new ResetMonitors(Profile) {Owner = this};
            bool? reset = resetDialog.ShowDialog();

            if (!reset.HasValue || !reset.Value)
            {
                return;
            }

            GetLoadingAdorner();

            try {
                ConfigManager.UndoManager.StartBatch();
                foreach (string progress in ResetMonitorsWork.ResetMonitors(this, resetDialog, Profile))
                {
                    if (!PumpUserInterface())
                    {
                        Logger.Warn("Aborting and rolling back any undoable operations from monitor reset");
                        ConfigManager.UndoManager.UndoBatch();
                        return;
                    }
                    ResetMonitorsWork.Logger.Debug(progress);
                }
                ConfigManager.UndoManager.CloseBatch();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Reset Monitors - Unhandled exception");
                Logger.Error("Rolling back any undoable operations from monitor reset");
                ConfigManager.UndoManager.UndoBatch();
                MessageBox.Show("Error encountered while resetting monitors; please file a bug with the contents of the application log", "Error");
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(RemoveLoadingAdorner));
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(OnResetMonitorsComplete));
            }
        }

        /// <summary>
        /// callback to main thread after reset monitors thread completes and all change events on Profile.Monitors collection
        /// and properties on Monitor objects have been delivered
        /// </summary>
        private void OnResetMonitorsComplete()
        {
            foreach (IResetMonitorsObserver interestedInterface in Profile.Interfaces.OfType<IResetMonitorsObserver>())
            {
                interestedInterface.NotifyResetMonitorsComplete();
            }
        }

        private void DockManager_Loaded(object sender, RoutedEventArgs e)
        {
            StringWriter systemDefaultLayoutWriter = new StringWriter();
            _layoutSerializer.Serialize(systemDefaultLayoutWriter);
            _systemDefaultLayout = systemDefaultLayoutWriter.ToString();
        }

        private void Donate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/BlueFinBima/Helios/wiki/Donations");
        }

        private void Explorer_ItemDeleting(object sender, ItemDeleteEventArgs e)
        {
            CloseProfileItem(e.DeletedItem);
        }
    }
}

