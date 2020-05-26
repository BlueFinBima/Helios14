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

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// Helios interfaces that implement this interface can be asked to resolve a binding target or source by name
    /// even if that binding target or source has not been previously declared.  This allows the creation of
    /// interfaces that can handle bindings configuration written for a different version of the interface or
    /// for an interface that isn't available.
    /// </summary>
    public interface IDynamicBindings
    {
        IBindingTrigger ResolveTrigger(string triggerId);
        IBindingAction ResolveAction(string actionId);
    }
}