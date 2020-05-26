namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    public interface IRotaryControl
    {
        /// <summary>
        /// </summary>
        /// <value>the new absolute orientation of the control in degrees</value>
        double ControlAngle { get; set; }
    }
}