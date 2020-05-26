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

using System;
using System.Diagnostics;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// this is a slightly modified version of the RelayCommand originally from
    /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern
    /// The purpose of this class is to allow a ViewModel class to implement ICommand against a number of
    /// lambdas, to make it easy to bind Command from a view to a full ICommand implementation in the ViewModel, knowing
    /// only its name.  Instances of this class will typically be lazy created in a property getter, so their lambda can
    /// reference data members.  There is no routing for this type of binding, because it has to be implemented exactly in the
    /// viewmodel.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        #endregion // Fields 

        #region Constructors

        public RelayCommand(Action<object> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        #endregion // Constructors 

        #region ICommand Members

        [DebuggerStepThrough]
        public virtual bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public virtual void Execute(object parameter)
        {
            _execute(parameter);
        }

        #endregion // ICommand Members 
    }

    //
    // snippet for canonical use:
    //
    /*
    <? xml version="1.0" encoding="utf-8"?>
    <CodeSnippet Format = "1.0.0" xmlns="http://schemas.microsoft.com/VisualStudio/2005/CodeSnippet">
    <Header>
    <Title>RelayCommand property, readonly</Title>
    <Author>derammo</Author>
    <Shortcut>relaycommand</Shortcut>
    <Description>ICommand Property implemented as RelayCommand to call lambdas in view model instance.</Description>
    <SnippetTypes>
    <SnippetType>Expansion</SnippetType>
    </SnippetTypes>
    </Header>
    <Snippet>
    <Declarations>
    <Literal>
    <ID>Documentation</ID>
    <Default>document this</Default>
    </Literal>
    <Literal>
    <ID>CommandName</ID>
    <Default>Some</Default>
    </Literal>
    <Literal>
    <ID>Private</ID>
    <Default>some</Default>
    </Literal>
    <Literal>
    <ID>Code</ID>
    <Default>Member();</Default>
    </Literal>
    </Declarations>
    <Code Language = "CSharp" >
    < ![CDATA[/// <summary>
        /// backing field for property $CommandName$Command, contains
        /// $Documentation$
        /// </summary>
        private ICommand _$Private$Command;
        
    /// <summary>
    /// $Documentation$
    /// </summary>
    public ICommand $CommandName$Command
    {
    get
    {
        _$Private$Command = _$Private$Command ?? new RelayCommand(parameter => { $Code$ });
        return _$Private$Command;
    }
    }]]>
    </Code>
    </Snippet>
    </CodeSnippet>
    */
}