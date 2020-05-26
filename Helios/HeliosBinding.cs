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

using System.Collections.Generic;

namespace GadrocsWorkshop.Helios
{
    using System;

    using NLua;
    using NLua.Exceptions;

    public class HeliosBinding : NotificationObject
    {
        private Lua _luaInterpreter;

        private bool _executing = false;

        private bool _active = true;
        private WeakReference _triggerSource = new WeakReference(null);
        private WeakReference _targetAction = new WeakReference(null);
        private bool _bypassTargetTriggers;
        private BindingValueSources _valueSource;
        private BindingValue _value = BindingValue.Empty;
        private string _condition = "";

        private bool _needsConversion = false;
        private BindingValueUnitConverter _converter = null;

        private bool _valid = false;
        private string _error = "";

#if DEVELOPMENT_CONFIGURATION
        private static Object _sTracingSource = null;
        private static bool _sTraceLoop = false;
#endif
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // deserialization constructor
        public HeliosBinding()
        {
        }

        // constructor used when building new binding
        public HeliosBinding(IBindingTrigger trigger, IBindingAction action)
        {
            Trigger = trigger;
            Action = action;
        }

        #region Properties

        public string ErrorMessage
        {
            get
            {
                return _error;
            }
            private set
            {
                if ((_error == null && value != null)
                    || (_error != null && !_error.Equals(value)))
                {
                    string oldValue = _error;
                    _error = value;
                    OnPropertyChanged("ErrorMessage", oldValue, value, false);
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return _valid;
            }
            private set
            {
                if (!_valid.Equals(value))
                {
                    bool oldValue = _valid;
                    _valid = value;
                    OnPropertyChanged("IsValid", oldValue, value, false);
                }
            }
        }

        public bool IsActive
        {
            get
            {
                return _active;
            }
            set
            {
                if (!_active.Equals(value))
                {
                    bool oldValue = _active;
                    _active = value;
                    OnPropertyChanged("IsActive", oldValue, value, false);
                }
            }
        }

        public bool HasCondition
        {
            get
            {
                return Condition != null && Condition.Trim().Length > 0;
            }
        }

        public string Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                if ((_condition == null && value != null)
                    || (_condition != null && !_condition.Equals(value)))
                {
                    string oldValue = _condition;
                    _condition = value;
                    OnPropertyChanged("Condition", oldValue, value, true);
                }
            }
        }

        public BindingValueSources ValueSource
        {
            get
            {
                return _valueSource;
            }
            set
            {
                if (!_valueSource.Equals(value))
                {
                    BindingValueSources oldValue = _valueSource;
                    _valueSource = value;
                    Validate();
                    OnPropertyChanged("ValueSource", oldValue, value, true);
                    OnPropertyChanged("Description", null, Description, false);
                    OnPropertyChanged("InputDescription", null, Description, false);
                    OnPropertyChanged("OutputDescription", null, Description, false);
                }
            }
        }

        public string Value
        {
            get
            {
                return _value.StringValue;
            }
            set
            {
                if ((_value == null && value != null)
                    || (_value != null && !_value.Equals(value)))
                {
                    string oldValue = _value.StringValue;
                    _value = new BindingValue(value);
                    OnPropertyChanged("Value", oldValue, value, true);
                    Validate();
                    if (ValueSource == BindingValueSources.StaticValue)
                    {
                        OnPropertyChanged("Description", null, Description, false);
                        OnPropertyChanged("InputDescription", null, Description, false);
                        OnPropertyChanged("OutputDescription", null, Description, false);
                    }
                }
            }
        }

        public IBindingTrigger Trigger
        {
            get
            {
                return _triggerSource.Target as IBindingTrigger;
            }
            set
            {
                IBindingTrigger oldTrigger = _triggerSource.Target as IBindingTrigger;
                if ((oldTrigger == null && value != null)
                    || (oldTrigger != null && !oldTrigger.Equals(value)))
                {

                    _triggerSource = new WeakReference(value);

                    UpdateConverter();
                    Validate();

                    OnPropertyChanged("Trigger", oldTrigger, value, true);
                    OnPropertyChanged("Description", null, Description, false);
                    OnPropertyChanged("InputDescription", null, Description, false);
                    OnPropertyChanged("OutputDescription", null, Description, false);
                }
            }
        }

        public IBindingAction Action
        {
            get
            {
                return _targetAction.Target as IBindingAction;
            }
            set
            {
                IBindingAction oldAction = _targetAction.Target as IBindingAction;

                if ((oldAction == null && value != null)
                    || (oldAction != null && !oldAction.Equals(value)))
                {
                    _targetAction = new WeakReference(value);

                    UpdateConverter();
                    Validate();

                    OnPropertyChanged("Action", oldAction, value, true);
                    OnPropertyChanged("Description", null, Description, false);
                    OnPropertyChanged("InputDescription", null, Description, false);
                    OnPropertyChanged("OutputDescription", null, Description, false);
                }
            }
        }

        public bool BypassCascadingTriggers
        {
            get
            {
                return _bypassTargetTriggers;
            }
            set
            {
                if (!_bypassTargetTriggers.Equals(value))
                {
                    bool oldBypass = _bypassTargetTriggers;
                    _bypassTargetTriggers = value;
                    OnPropertyChanged("BypassCascadingTriggers", oldBypass, value, true);
                }
            }
        }

        public string Description
        {
            get
            {
                if (IsValid)
                {
                    return ReplaceValue(Trigger.TriggerBindingDescription + " " + Action.ActionBindingDescription);
                }
                else
                {
                    return ErrorMessage;
                }
            }
        }

        public string InputDescription
        {
            get
            {
                if (IsValid)
                {
                    return ReplaceValue(Action.ActionInputBindingDescription + " " + Trigger.TriggerBindingDescription);
                }
                else
                {
                    return ErrorMessage;
                }
            }
        }

        public string OutputDescription
        {
            get
            {
                if (IsValid)
                {
                    return ReplaceValue(Action.ActionBindingDescription);
                }
                else
                {
                    return ErrorMessage;
                }
            }
        }

        private Lua LuaInterpreter
        {
            get
            {
                if (_luaInterpreter == null)
                {
                    _luaInterpreter = new Lua();
                }
                return _luaInterpreter;
            }
        }

        #endregion


        private string ReplaceValue(string inputString)
        {
            string valueString;
            switch (ValueSource)
            {
                case BindingValueSources.StaticValue:
                    valueString = Value;
                    break;
                case BindingValueSources.TriggerValue:
                    valueString = "trigger value";
                    break;
                case BindingValueSources.LuaScript:
                    valueString = "script results";
                    break;
                default:
                    valueString = "";
                    break;
            }

            return inputString.Replace("%value%", valueString);
        }

        private BindingValue CreateBindingValue(object value)
        {
            if (value is string)
            {
                return new BindingValue((string)value);
            }
            else if (value is double)
            {
                return new BindingValue((double)value);
            }
            else if (value is bool)
            {
                return new BindingValue((bool)value);
            }
            else if (value is long longValue)
            {
                // construct double from long
                return new BindingValue(longValue);
            }

            return BindingValue.Empty;
        }

#if DEVELOPMENT_CONFIGURATION
        private static System.Collections.Generic.HashSet<string> _sTracesReported = new System.Collections.Generic.HashSet<string>();

        private void TraceTriggerFired()
        {
            // NOTE: deliberately crash if any of these are null
            HeliosObject target = (_targetAction.Target as IBindingAction).Target;
            if (target._tracing || _executing)
            {
                if (!_sTracesReported.Contains(Description))
                {
                    string loopType = _executing ? "Hard" : "Soft";
                    Logger.Info($"{loopType} binding loop detected; Object {target.Name} may have triggered itself");
                    _sTraceLoop = true;
                    _sTracesReported.Add(Description);
                }
                _sTracingSource = target;
            }
            HeliosObject source = (_triggerSource.Target as IBindingTrigger).Source;
            source._tracing = true;
        }

        private void EndTraceTriggerFired()
        {
            if (_sTraceLoop)
            {
                Logger.Info($"  binding loop includes {Description}");
            }
            HeliosObject source = (_triggerSource.Target as IBindingTrigger).Source;
            if (source._tracing)
            {
                source._tracing = false;
                if (_sTracingSource == source)
                {
                    if (_sTraceLoop)
                    {
                        _sTraceLoop = false;
                        Logger.Info("  binding loop trace complete");
                    }
                    _sTracingSource = null;
                }
            }
        }
#else
        private void TraceTriggerFired()
        {
            // no code
        }
        private void EndTraceTriggerFired()
        {
            // no code
        }
#endif

        // log filters to log certain messages only once per binding, because otherwise we will crush the logger and 
        // keep the program from running under load
        private static readonly Util.OnceLogger TriggeredLogger = new Util.OnceLogger(Logger);
        private static readonly Util.OnceLogger LuaConditionLogger = new Util.OnceLogger(Logger);
        private static readonly Util.OnceLogger LuaValueLogger = new Util.OnceLogger(Logger);
        private static readonly Util.OnceLogger LuaNoValueLogger = new Util.OnceLogger(Logger);

        public void OnTriggerFired(object trigger, HeliosTriggerEventArgs e)
        {
            if (!IsActive)
            {
                return;
            }

            string loggingId = ((IBindingTrigger)_triggerSource.Target).TriggerID;
            TraceTriggerFired();
            if (_executing)
            {
                Logger.Warn("Binding loop condition detected, binding aborted. (Binding=\"" +
                                                    Description + "\")");
                EndTraceTriggerFired();
                return;
            }

            TriggeredLogger.InfoOnceUnlessDebugging(loggingId, "Binding triggered. (Binding={Description}, Value={Value})", Description, e.Value.StringValue);
            try
            {
                _executing = true;
                BindingValue value = BindingValue.Empty;
                //LuaInterpreter["TriggerValue"] = e.Value.NaitiveValue;
                switch (e.Value.NaitiveType)
                {
                    case BindingValueType.Boolean:
                        LuaInterpreter["TriggerValue"] = e.Value.BoolValue;
                        break;
                    case BindingValueType.String:
                        LuaInterpreter["TriggerValue"] = e.Value.StringValue;
                        break;
                    case BindingValueType.Double:
                        LuaInterpreter["TriggerValue"] = e.Value.DoubleValue;
                        break;
                }

                if (HasCondition)
                {
                    try
                    {
                        object[] conditionReturnValues = LuaInterpreter.DoString(_condition);
                        if (conditionReturnValues.Length >= 1)
                        {
                            BindingValue returnValue = CreateBindingValue(conditionReturnValues[0]);
                            if (returnValue.BoolValue == false)
                            {
                                if (Logger.IsDebugEnabled)
                                {
                                    if (Logger.IsDebugEnabled)
                                    {
                                        Logger.Debug("Binding condition evaluated to false, binding {Binding} aborted.", Description);
                                    }
                                }
                                EndTraceTriggerFired();
                                return;
                            }
                        }
                    }
                    catch (LuaScriptException luaException)
                    {
                        LuaConditionLogger.WarnOnceUnlessDebugging(loggingId,
                            "Binding condition Lua error {Error} from script {Script} on binding {Binding}",
                            luaException.Message, Condition, Description);
                    }
                    catch (Exception conditionException)
                    {
                        Logger.Error(conditionException, "Binding condition has thown an unhandled exception. (Binding=\"" + Description + "\", Condition=\"" + Condition + "\")");
                        EndTraceTriggerFired();
                        return;
                    }
                }

                switch (ValueSource)
                {
                    case BindingValueSources.StaticValue:
                        value = _value;
                        break;
                    case BindingValueSources.TriggerValue:
                        if (_needsConversion && _converter != null)
                        {
                            value = _converter.Convert(e.Value, Trigger.Unit, Action.Unit);
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("Binding converted value. (Binding=\"" + Description + "\", Original Value=\"" + e.Value.StringValue + "\", New Value=\"" + value.StringValue + "\")");
                            }
                        }
                        else
                        {
                            value = e.Value;
                        }
                        break;
                    case BindingValueSources.LuaScript:
                        try
                        {
                            object[] returnValues = LuaInterpreter.DoString(Value);
                            if ((returnValues != null) && (returnValues.Length >= 1))
                            {
                                value = CreateBindingValue(returnValues[0]);
                                if (Logger.IsDebugEnabled)
                                {
                                    Logger.Debug("Binding value Lua script evaluated (Binding=\"" + Description + "\", Expression=\"" + Value + "\", TriggerValue=\"" + LuaInterpreter["TriggerValue"] + "\", ReturnValue=\"" + returnValues[0] + "\")");
                                }
                            }
                            else
                            {
                                LuaNoValueLogger.WarnOnceUnlessDebugging(loggingId,
                                    "Binding value Lua script did not return a value from script {Script} for trigger value {TriggerValue} on binding {Binding}",
                                    Value, LuaInterpreter["TriggerValue"], Description);
                            }
                        }
                        catch (LuaScriptException luaException)
                        {
                            LuaValueLogger.WarnOnceUnlessDebugging(loggingId,
                                "Binding value Lua error {Error} from script {Script} for trigger value {TriggerValue} on binding {Binding}",
                                luaException.Message, Value, LuaInterpreter["TriggerValue"], Description);
                        }
                        catch (Exception valueException)
                        {
                            // these are exceptions thrown by the Lua implementation
                            Logger.Error(valueException, "Binding value Lua script has thown an unhandled exception. (Binding=\"" + Description + "\", Value Script=\"" + Value + "\")");
                        }
                        break;
                }
                if(Action.Target.Dispatcher == null) {
                    // if we don't have a dispatcher, likely because this is a composite device, we use the dispatcher object from the owner of the trigger
                    // this seems to work and was the best I could come up with, but it is very much a kludge because I could not work out the proper way
                    // to have a dispatcher created for Actions
                    Action.Target.Dispatcher = Trigger.Owner.Dispatcher;
                }
                Action.Target.Dispatcher.Invoke(new Action<BindingValue, bool>(Execute), value, BypassCascadingTriggers);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Binding threw unhandled exception. (Binding=\"" + Description + "\")");
            }
            _executing = false;
            EndTraceTriggerFired();
        }

        private void Execute(BindingValue value, bool bypass)
        {
            Action.ExecuteAction(value, bypass);
        }

        public void Clone(HeliosBinding binding)
        {
            Trigger = binding.Trigger;
            Action = binding.Action;
            ValueSource = binding.ValueSource;
            Value = binding.Value;
            BypassCascadingTriggers = binding.BypassCascadingTriggers;
        }

        private void UpdateConverter()
        {
            if (Trigger != null && Action != null)
            {
                _needsConversion = !Trigger.Unit.Equals(Action.Unit);
                _converter = ConfigManager.ModuleManager.GetUnitConverter(Trigger.Unit, Action.Unit);
            }
            else
            {
                _needsConversion = false;
                _converter = null;
            }
        }

        private void Validate()
        {
            if (Trigger == null)
            {
                IsValid = false;
                ErrorMessage = "Invalid Trigger - Please select a new trigger event.";
                return;
            }

            if (Action == null)
            {
                IsValid = false;
                ErrorMessage = "Invalid Action - Please select a new action.";
                return;
            }

            if (Action.ActionRequiresValue)
            {
                if (ValueSource == BindingValueSources.TriggerValue && _needsConversion && _converter == null)
                {
                    IsValid = true;
                    ErrorMessage = "Action Value Warning - Cannot convert trigger value to action value.";
                    return;
                }

                if (ValueSource == BindingValueSources.StaticValue && (Value == null || Value.Length == 0))
                {
                    IsValid = true;
                    ErrorMessage = "Action Value Warning - Value cannot be empty.";
                    return;
                }

                if (ValueSource == BindingValueSources.LuaScript && (Value == null || Value.Length == 0))
                {
                    IsValid = true;
                    ErrorMessage = "Action Value Warning - Script cannot be empty.";
                    return;
                }
            }

            if (ErrorMessage != null && ErrorMessage.Length > 0)
            {
                IsValid = true;
                ErrorMessage = "";
            }
        }
    }
}
