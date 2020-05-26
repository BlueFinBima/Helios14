// Copyright 2020 Helios Contributors
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GadrocsWorkshop.Helios.Windows;
using NLog;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    // model and view model for the status viewer
    public class StatusViewer : DependencyObject
    {
        // about two lines of status message are allowed, the rest will be cut if from log
        private const int STATUS_LIMIT = 120;

        private readonly Queue<StatusViewerItem> _items = new Queue<StatusViewerItem>();
        private readonly LinkedList<StatusViewerItem> _shown = new LinkedList<StatusViewerItem>();
        private readonly HashSet<string> _uniqueLogMessages = new HashSet<string>();

        // maximum
        private readonly int _capacity = 200;

        // these currently do nothing, but would support internal scrolling, i.e. having more items stored than we give to WPF
        // REVISIT remove or actually use
        private int _windowBase;
        private readonly int _windowSize;

        public class StatusTemplateSelector : DataTemplateSelector
        {
            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                StatusViewerItem listItem = item as StatusViewerItem;
                FrameworkElement element = container as FrameworkElement;
                if (listItem == null)
                {
                    return null;
                }

                StatusReportItem.SeverityCode severity = listItem.Data.Severity;
                switch (severity)
                {
                    case StatusReportItem.SeverityCode.None:
                        // these are incorrectly initialized
                        ConfigManager.LogManager.LogError(
                            $"received status report item with invalid severity: {listItem.Data.Severity} '{listItem.Data.Status}'; implementation error");
                        severity = StatusReportItem.SeverityCode.Error;
                        break;
                }

                return element.FindResource(severity.ToString()) as DataTemplate;
            }
        }

        public StatusViewer()
        {
            // as long as we don't use these, this needs to be set to max
            _windowSize = _capacity;

            // don't use default generated dependency property, we need our own copy
            Items = new ObservableCollection<StatusViewerItem>();

            // register as a log consumer (NOTE: we never deregister)
            StatusViewerLogTarget.Parent = this;
        }

        /// <summary>
        /// backing field for property ClearCommand, contains
        /// handler for Clear action
        /// </summary>
        private ICommand _clearCommand;

        /// <summary>
        /// handler for Clear action
        /// </summary>
        public ICommand ClearCommand
        {
            get
            {
                _clearCommand = _clearCommand ?? new RelayCommand(parameter => { Clear(); });
                return _clearCommand;
            }
        }

        /// <summary>
        /// backing field for property ShareCommand, contains
        /// handler for share action
        /// </summary>
        private ICommand _shareCommand;

        /// <summary>
        /// handler for share action
        /// </summary>
        public ICommand ShareCommand
        {
            get
            {
                _shareCommand = _shareCommand ?? new RelayCommand(parameter =>
                {
                    // stop the profile because we need to use the screen without getting
                    // covered up
                    ControlCenterCommands.StopProfile.Execute(null, parameter as IInputElement);

                    // execute a modal dialog
                    Dialog.ShowModalCommand.Execute(
                        new ShowModalParameter
                        {
                            Content = new ShareConsoleStatus(new List<ConsoleStatus>
                            {
                                new ConsoleStatus(Items.Select(v => v.Data))
                            })
                        },
                        parameter as IInputElement);
                });
                return _shareCommand;
            }
        }

        public void AddItem(StatusReportItem item)
        {
            switch (item.Severity)
            {
                case StatusReportItem.SeverityCode.Info:
                    if (item.Flags.HasFlag(StatusReportItem.StatusFlags.Verbose))
                    {
                        // too numerous for this console
                        return;
                    }

                    break;
                case StatusReportItem.SeverityCode.Warning:
                // fall through
                case StatusReportItem.SeverityCode.Error:
                case StatusReportItem.SeverityCode.None:
                    CautionLightVisibility = Visibility.Visible;
                    break;
            }

            StatusViewerItem viewModel = new StatusViewerItem(item);

            // store it for display
            _items.Enqueue(viewModel);

            // if visible, display new item
            if (_windowBase + _windowSize >= _items.Count)
            {
                _shown.AddLast(viewModel);
                Items.Add(viewModel);

                // may have scrolled something off
                while (_shown.Count > _windowSize)
                {
                    Items.Remove(_shown.First.Value);
                    _shown.RemoveFirst();
                }
            }

            // finally, check if we exceeded our capacity
            while (_items.Count > _capacity)
            {
                StatusViewerItem discard = _items.Dequeue();
                if (_windowBase == 0)
                {
                    Items.Remove(discard);
                    _shown.RemoveFirst();
                }

                _windowBase--;
            }
        }

        public void Clear()
        {
            _items.Clear();
            _shown.Clear();
            _uniqueLogMessages.Clear();
            _windowBase = 0;
            Items.Clear();
            ResetCautionLight();
        }

        public void ResetCautionLight()
        {
            CautionLightVisibility = Visibility.Hidden;
        }

        public void WriteLogMessage(LogEventInfo eventInfo, string message)
        {
            StatusReportItem.SeverityCode code;
            if (eventInfo.Level == NLog.LogLevel.Warn)
            {
                code = StatusReportItem.SeverityCode.Warning;
            }
            else if (eventInfo.Level == NLog.LogLevel.Error)
            {
                code = StatusReportItem.SeverityCode.Error;
            }
            else
            {
                // don't include info messages
                return;
            }

            if (_uniqueLogMessages.Contains(message))
            {
                // don't include a message more than once
                return;
            }

            _uniqueLogMessages.Add(message);
            // shorten multiline messages, taking at most one line
            string trimmedMessage = message;
            if (trimmedMessage.Length > STATUS_LIMIT)
            {
                trimmedMessage = message.Substring(0, STATUS_LIMIT);
            }

            int newline = trimmedMessage.IndexOf('\n');
            while (newline >= 0)
            {
                if (newline == 0)
                {
                    // trim from front and continue
                    trimmedMessage = trimmedMessage.Substring(1);
                    newline = trimmedMessage.IndexOf('\n');
                }
                else
                {
                    // found something
                    trimmedMessage = trimmedMessage.Substring(0, newline);
                    break;
                }
            }

            string recommendation = null;
            string fullText = null;
            if (trimmedMessage.Length < message.Length)
            {
                recommendation = "Log message was shortened.  See application log for details.";
                fullText = message;
            }

            if (eventInfo.Exception != null)
            {
                recommendation = $"An exception was thrown: '{eventInfo.Exception.Message}'.  You should file a bug.";
            }

            StatusReportItem item = new StatusReportItem
            {
                TimeStamp = StatusReportItem.CreateTimeStamp(eventInfo.TimeStamp),
                Severity = code,
                Status = trimmedMessage,
                Recommendation = recommendation,
                FullText = fullText,
                Flags = StatusReportItem.StatusFlags.TimeStampIsPrecise
            };
            AddItem(item);
        }

        public StatusTemplateSelector TemplateSelector { get; } = new StatusTemplateSelector();

        /// <summary>
        /// state of master caution light
        /// </summary>
        public Visibility CautionLightVisibility
        {
            get => (Visibility) GetValue(CautionLightVisibilityProperty);
            set => SetValue(CautionLightVisibilityProperty, value);
        }

        public static readonly DependencyProperty CautionLightVisibilityProperty =
            DependencyProperty.Register("CautionLightVisibility", typeof(Visibility), typeof(StatusViewer),
                new PropertyMetadata(Visibility.Hidden));

        public ObservableCollection<StatusViewerItem> Items
        {
            get => (ObservableCollection<StatusViewerItem>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("observableCollection", typeof(ObservableCollection<StatusViewerItem>),
                typeof(StatusViewer), new PropertyMetadata(null));
    }
}