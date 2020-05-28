//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later versionCannot find interface trigger
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
    using System.Windows;
    using System.Windows.Media;
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;


    public abstract partial class AV8BCompositeVisual : CompositeVisual
    {
        protected new HeliosInterface _defaultInterface;

        public AV8BCompositeVisual(string name, Size nativeSize)
            : base(name, nativeSize)
        {
            _defaultInterfaceName = "";
            _defaultBindingName = "";
            _defaultInterface = null;
        }

        //
        // For gauges with more than one input function, we need this override to map the 
        // different interface element names to the individual functions.  This carries with it
        // a requirement for the gauge function name to equal the interface element name
        //

        protected new Gauges.BaseGauge AddGauge(
            string name,
            Gauges.BaseGauge gauge,
            Point posn,
            Size size,
            string interfaceDeviceName,
            string interfaceElementName
            ) => AddGauge(name, gauge, posn, size, interfaceDeviceName, new string[1] { interfaceElementName });
        protected Gauges.BaseGauge AddGauge(
            string name,
            Gauges.BaseGauge gauge,
            Point posn,
            Size size,
            string interfaceDeviceName,
            string[] interfaceElementNames
            )
        {
            if (DefaultInterfaceName == "DCS AV-8B" && (name == "Altimeter Gauge" || name == "ADI Gauge" || name == "Slip/Turn Gauge"|| name == "AOA Gauge"))
            {
                gauge.Name = name;
                gauge.Top = posn.Y;
                gauge.Left = posn.X;
                gauge.Width = size.Width;
                gauge.Height = size.Height;

                string componentName = GetComponentName(name);
                gauge.Name = componentName;


                Children.Add(gauge);
                foreach (IBindingTrigger trigger in gauge.Triggers)
                {
                    AddTrigger(trigger, trigger.Device);
                }
                int i = 0;
                foreach (IBindingAction action in gauge.Actions)
                {
                    if (action.Name != "hidden")
                    {
                        AddAction(action, action.Device);
                        
                        AddDefaultInputBinding(
                        childName: componentName,
                        interfaceTriggerName: interfaceDeviceName + "." + interfaceElementNames[i++] + ".changed",
                        deviceActionName: action.Device + "." + action.ActionVerb + "." + action.Name
                        );
                    }
                }
                return gauge;
            }
            else
            {
                return base.AddGauge(
                    name,
                    gauge,
                    posn,
                    size,
                    interfaceDeviceName,
                    interfaceElementNames[0]
                    );
            }
        }
        private Point FromCenter(Point posn, Size size)
        {
            return new Point(posn.X - size.Width / 2.0, posn.Y - size.Height / 2.0);
        }
        protected new RotaryEncoder AddEncoder(string name, Point posn, Size size,
            string knobImage, double stepValue, double rotationStep,
            string interfaceDeviceName, string interfaceElementName, bool fromCenter)
        {
            if (fromCenter)
                posn = FromCenter(posn, size);
            string componentName = GetComponentName(name);
            RotaryEncoder _knob = new RotaryEncoder
            {
                Name = componentName,
                KnobImage = knobImage,
                StepValue = stepValue,
                RotationStep = rotationStep,
                Top = posn.Y,
                Left = posn.X,
                Width = size.Width,
                Height = size.Height
            };

            Children.Add(_knob);
            foreach (IBindingTrigger trigger in _knob.Triggers)
            {
                AddTrigger(trigger, componentName);
            }
            foreach (IBindingAction action in _knob.Actions)
            {
                if (action.Name != "hidden")
                {
                    AddAction(action, componentName);
                }
            }
            AddDefaultOutputBinding(
                childName: componentName,
                deviceTriggerName: "encoder.incremented",
                interfaceActionName: interfaceDeviceName + ".increment." + interfaceElementName
            );
            AddDefaultOutputBinding(
                childName: componentName,
                deviceTriggerName: "encoder.decremented",
                interfaceActionName: interfaceDeviceName + ".decrement." + interfaceElementName
                );

            return _knob;
        }
        protected ToggleSwitch AddToggleSwitch(string name, Point posn, Size size, ToggleSwitchPosition defaultPosition,
            string positionOneImage, string positionTwoImage, ToggleSwitchType defaultType, LinearClickType clickType, string interfaceDeviceName, string interfaceElementName,
            bool fromCenter, bool horizontal = false, string interfaceIndicatorElementName = "")
        {
            if (fromCenter)
                posn = FromCenter(posn, size);
            string componentName = GetComponentName(name);

            ToggleSwitch newSwitch = new ToggleSwitch(); 
            newSwitch.Name = componentName;
            newSwitch.SwitchType = defaultType;
            newSwitch.ClickType = clickType;
            newSwitch.DefaultPosition = defaultPosition;
            newSwitch.HasIndicator = true;
            if (interfaceIndicatorElementName != "")
            {
                // if there is an indicatorElementname then the image names will be partial
                // and need to be completed
                newSwitch.PositionOneImage = positionOneImage + " off.png";
                newSwitch.PositionTwoImage = positionTwoImage + " off.png";
                newSwitch.PositionOneIndicatorOnImage = positionOneImage + " on.png";
                newSwitch.PositionTwoIndicatorOnImage = positionTwoImage + " on.png";
                newSwitch.HasIndicator = true;
            }
            else
            {
                newSwitch.PositionOneImage = positionOneImage;
                newSwitch.PositionTwoImage = positionTwoImage;
                newSwitch.HasIndicator = false;
            }
            newSwitch.Width = size.Width;
            newSwitch.Height = size.Height;

            newSwitch.Top = posn.Y;
            newSwitch.Left = posn.X;
            if (horizontal)
            {
                newSwitch.Rotation = HeliosVisualRotation.CW;
                newSwitch.Orientation = ToggleSwitchOrientation.Horizontal;
            }

            Children.Add(newSwitch);

            foreach (IBindingTrigger trigger in newSwitch.Triggers)
            {
                AddTrigger(trigger, componentName);
            }
            AddAction(newSwitch.Actions["set.position"], componentName);

            AddDefaultOutputBinding(
                childName: componentName,
                deviceTriggerName: "position.changed",
                interfaceActionName: interfaceDeviceName + ".set." + interfaceElementName
            );
            AddDefaultInputBinding(
                childName: componentName,
                interfaceTriggerName: interfaceDeviceName + "." + interfaceElementName + ".changed",
                deviceActionName: "set.position");

            if (newSwitch.HasIndicator)
            {
                AddAction(newSwitch.Actions["set.indicator"], componentName);

                AddDefaultInputBinding(
                    childName: componentName,
                    interfaceTriggerName: interfaceDeviceName + "." + interfaceIndicatorElementName + ".changed",
                    deviceActionName: "set.indicator");
            }
            return newSwitch;
        }


    }
}
