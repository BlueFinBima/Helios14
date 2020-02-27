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

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    public class HeliosTextBox : TextBox
    {
        CommandBinding undoBinding;  
        CommandBinding redoBinding;

        public HeliosTextBox()  
        {  
            UndoLimit = 1;  
        }  
 
        // our UI assumes this attached property is always present and non-empty, so we have to wrap it
        public string ErrorToolTip
        {
            get
            {
                System.Collections.Generic.IList<ValidationError> errors = Validation.GetErrors(this);
                if (errors == null)
                {
                    return "";
                }
                if (errors.Count > 0)
                {
                    return errors[0].ErrorContent as string;
                }
                return "";
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)  
        {  
            if (undoBinding == null)  
            {  
                undoBinding = new CommandBinding(  
                    ApplicationCommands.Undo, new ExecutedRoutedEventHandler(UndoExecuted), null);  
                redoBinding = new CommandBinding(  
                    ApplicationCommands.Redo, new ExecutedRoutedEventHandler(RedoExecuted), null);  
 
                CommandBindings.Add(undoBinding);  
                CommandBindings.Add(redoBinding);  
            }
            base.OnTextChanged(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                BindingExpression exp = GetBindingExpression(TextBox.TextProperty);
                exp.UpdateSource();
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            BindingExpression exp = GetBindingExpression(TextBox.TextProperty);
            exp.UpdateSource();
            base.OnLostKeyboardFocus(e);
        }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs args)  
        {  
            ApplicationCommands.Undo.Execute(null, Application.Current.MainWindow);  
        }  
 
        private void RedoExecuted(object sender, ExecutedRoutedEventArgs args)  
        {  
            ApplicationCommands.Redo.Execute(null, Application.Current.MainWindow);  
        } 
    }
}
