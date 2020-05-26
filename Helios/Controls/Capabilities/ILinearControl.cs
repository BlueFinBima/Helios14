namespace GadrocsWorkshop.Helios.Controls.Capabilities
{
    public interface ILinearControl
    {
        /// <summary>
        /// the position of the control as a multiple of its declared range, so that
        /// 1.0 is the configured maximum for the control and 0.0 is the configured minimum
        ///
        /// setting the value outside the range must be handled gracefully by the control by
        /// wrapping or clamping, depending on the type of control it is
        /// </summary>
        double ControlPosition { get; set; }
    }
}