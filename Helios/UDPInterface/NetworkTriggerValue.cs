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

using System;

namespace GadrocsWorkshop.Helios.UDPInterface
{
    // a network function that presents as a changeable string value
    public class NetworkTriggerValue : NetworkFunction
    {
        private readonly string _id;
        private readonly HeliosValue _value;
        private readonly HeliosTrigger _receivedTrigger;

        public class Value : EventArgs
        {
            public string Text { get; set; }
        }

        // fired when a value is received, in addition to firing any Helios bindings
        public event EventHandler<Value> ValueReceived;

        public NetworkTriggerValue(BaseUDPInterface sourceInterface, string id, string name, string description,
            string valueDescription)
            : base(sourceInterface)
        {
            _id = id;
            _value = new HeliosValue(sourceInterface, BindingValue.Empty, "", name, description, valueDescription,
                BindingValueUnits.Text);
            Values.Add(_value);
            Triggers.Add(_value);
            _receivedTrigger = new HeliosTrigger(sourceInterface, "", name, "received", description);
        }

        // optional additional trigger for received event regardless of whether the data changes
        public HeliosTrigger Received() => _receivedTrigger;

        public override void ProcessNetworkData(string id, string value)
        {
            BindingValue bound = new BindingValue(value);
            _value.SetValue(bound, false);
            _receivedTrigger.FireTrigger(bound);
            ValueReceived?.Invoke(this, new Value {Text = value});
        }

        public override ExportDataElement[] GetDataElements()
        {
            return new[] {new ExportDataElement(_id)};
        }

        public override void Reset()
        {
            _value.SetValue(BindingValue.Empty, true);
        }
    }
}