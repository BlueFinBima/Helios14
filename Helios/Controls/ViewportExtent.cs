using GadrocsWorkshop.Helios.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace GadrocsWorkshop.Helios.Controls
{
    [HeliosControl("Helios.Base.ViewportExtent", "Simulator Viewport", "Miscellaneous", typeof(ViewportExtentRenderer))]
    public class ViewportExtent: TextDecoration, IViewportExtent
    {
        private const string DEFAULT_NAME = "Simulator Viewport";
        private const string DEFAULT_TEXT = "Label";
        private const string DEFAULT_VIEWPORT_NAME = "";
        private string _viewportName = DEFAULT_VIEWPORT_NAME;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ViewportExtent()
            : base(DEFAULT_NAME)
        {
            FillBackground = true;
            BackgroundColor = Color.FromArgb(128, 128, 0, 0);
            FontColor = Color.FromArgb(255, 255, 255, 255);
            Format.VerticalAlignment = TextVerticalAlignment.Center;
        }

        public bool RequiresPatches { get; set; }

        public string ViewportName
        {
            get => _viewportName;
            set
            {
                string oldValue = _viewportName;
                _viewportName = value;
                if ((Name == DEFAULT_NAME) || (Name == oldValue))
                {
                    // XXX this fails to update the UI, put a diag on it and find out why
                    Name = value;
                }
                if ((Text == DEFAULT_TEXT) || (Text == oldValue))
                {
                    Text = value;
                }
            }
        }

        public override bool HitTest(Point location)
        {
            // only design time
            return ConfigManager.Application.ShowDesignTimeControls;
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            System.ComponentModel.TypeConverter bc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(bool));
            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ViewportName":
                        ViewportName = reader.ReadElementContentAsString();
                        break;
                    case "RequiresPatches":
                        RequiresPatches = (bool)bc.ConvertFromInvariantString(reader.ReadElementString("RequiresPatches"));
                        break;
                    case "Children":
                        // leave this for our caller
                        return;
                    default:
                        // ignore unsupported settings
                        string elementName = reader.Name;
                        string discard = reader.ReadElementString(elementName);
                        Logger.Warn($"Ignored unsupported {GetType().Name} setting '{elementName}' with value '{discard}'");
                        break;
                }
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            System.ComponentModel.TypeConverter bc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(bool));
            if (ViewportName != DEFAULT_VIEWPORT_NAME)
            {
                writer.WriteElementString("ViewportName", ViewportName);
            }
            if (RequiresPatches)
            {
                writer.WriteElementString("RequiresPatches", bc.ConvertToInvariantString(RequiresPatches));
            }
        }
    }
}
