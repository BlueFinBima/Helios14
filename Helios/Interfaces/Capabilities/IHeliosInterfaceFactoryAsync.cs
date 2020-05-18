namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    public interface IAvailableInterfaces
    {
        /// <summary>
        /// callback to indicate a factory has discovered another instance that is not present in
        /// the profile and which can be created on demand
        ///
        /// WARNING: called back on an arbitrary thread
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="displayName"></param>
        /// <param name="context"></param>
        void ReceiveAvailableInstance(IHeliosInterfaceFactoryAsync factory, string displayName, object context);
    }

    /// <summary>
    /// Descendants of HeliosInterfaceFactory can implement this interface to defer the discovery and creation of interface 
    /// instances and allow the use of other threads to perform the discovery itself
    /// </summary>
    public interface IHeliosInterfaceFactoryAsync
    {
        void StartDiscoveringInterfaces(IAvailableInterfaces callbacks, HeliosProfile profile);
        HeliosInterface CreateInstance(object context);
    }
}