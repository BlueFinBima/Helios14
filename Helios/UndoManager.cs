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

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using GadrocsWorkshop.Helios.ComponentModel;

    public class UndoManager
    {
        private static int MAX_UNDO_ITEMS = 2000; 

        private bool _working = false;
        private UndoManagerBatch _batch = null;

        private Stack<IUndoItem> _undoEvents;
        private Stack<IUndoItem> _redoEvents;

        public UndoManager()
        {
            _undoEvents = new Stack<IUndoItem>();
            _redoEvents = new Stack<IUndoItem>();
        }

        /// <summary>
        ///  Gets a value indicating whether there is anything that can be undone.
        /// </summary>
        public bool CanUndo { get { return _undoEvents.Count > 0 && !_working && _batch == null; } }

        /// <summary>
        /// Gets a value indicating whether there is anything that can be rolled forward.
        /// </summary>
        public bool CanRedo { get { return _redoEvents.Count > 0 && !_working && _batch == null; } }

        /// <summary>
        /// Rollback the last command.
        /// </summary>
        public void Undo()
        {
            if (CanUndo)
            {
                _working = true;
                IUndoItem undo = _undoEvents.Pop();
                undo.Undo();
                _redoEvents.Push(undo);
                _working = false;
            }
        }

        /// <summary>
        /// Rollback the last undone command.
        /// </summary>
        public void Redo()
        {
            if (CanRedo)
            {
                _working = true;
                IUndoItem redo = _redoEvents.Pop();
                redo.Do();
                _undoEvents.Push(redo);
                _working = false;
            }
        }

        /// <summary>
        /// Clear the undo history.
        /// </summary>
        public void ClearUndoHistory()
        {
            _undoEvents.Clear();
        }

        /// <summary>
        /// Clear the redo history.
        /// </summary>
        public void ClearRedoHistory()
        {
            _redoEvents.Clear();
        }

        /// <summary>
        /// Clear all the undo and redo history.
        /// </summary>
        public void ClearHistory()
        {
            ClearRedoHistory();
            ClearUndoHistory();
        }

        public void AddUndoItem(IUndoItem undoEvent)
        {
            if (!_working)
            {
                if (_batch != null)
                {
                    _batch.Add(undoEvent);
                }
                else
                {
                    ClearRedoHistory();
                    _undoEvents.Push(undoEvent);
                    if (_undoEvents.Count > MAX_UNDO_ITEMS)
                    {
                        _undoEvents.Pop();
                    }
                }
            }
        }

        public void AddPropertyChange(object source, string propertyName, object oldValue, object newValue)
        {
            if (!_working)
            {
                Type type = source.GetType();
                PropertyInfo property = type.GetProperty(propertyName);
                if (property.CanWrite)
                {
                    AddUndoItem(new PropertyChangedUndoItem(source, property, oldValue, newValue));
                }
            }
        }

        public void AddPropertyChange(object sender, PropertyNotificationEventArgs notification)
        {
            if (notification != null && notification.IsUndoable)
            {
                while (notification.HasChildNotification)
                {
                    notification = notification.ChildNotification;
                }
                AddPropertyChange(notification.EventSource, notification.PropertyName, notification.OldValue, notification.NewValue);
            }
        }

        public void StartBatch()
        {
            if (_batch == null)
            {
                ClearRedoHistory();
                _batch = new UndoManagerBatch();
            }
        }

        public void CloseBatch()
        {
            if (_batch != null)
            {
                if (_batch.Count > 0)
                {
                    _undoEvents.Push(_batch);
                }
                _batch = null;
            }
        }

        /// <summary>
        /// roll back everything in the currently open batch, not redoable
        /// </summary>
        public void UndoBatch()
        {
            _working = true;
            try
            {
                _batch?.Undo();
            }
            finally
            {
                _working = false;
                _batch = null;
            }
        }
    }
}
