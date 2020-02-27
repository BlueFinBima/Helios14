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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    using GadrocsWorkshop.Helios.UDPInterface;

    public class SilentValueConsumer : NetworkFunction
    {
        private string _id;

        public SilentValueConsumer(BaseUDPInterface sourceInterface, string id, string description)
            : base(sourceInterface)
        {
            _id = id;
        }

        public override void ProcessNetworkData(string id, string value)
        {
            // do nothing with it, just avoid logging it as a warning
        }

        public override ExportDataElement[] GetDataElements()
        {
            // return a data element that is not a DCSDataElement, so it does not generate an export
            // but still consumes its value
            return new ExportDataElement[] { new ExportDataElement(_id) };
        }

        public override void Reset()
        {
            // no code
        }
    }
}
