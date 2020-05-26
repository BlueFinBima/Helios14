namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    public interface IPulsedControl
    {
        /// <summary>
        /// increment (positive pulses) or decrement (negative pulses) the control
        /// </summary>
        /// <param name="pulses">the number of pulses, negative if decrementing</param>
        void Pulse(int pulses);
    }
}