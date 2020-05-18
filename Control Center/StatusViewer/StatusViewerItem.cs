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

using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    public class StatusViewerItem : HeliosStaticViewModel<StatusReportItem>
    {
        public StatusViewerItem(StatusReportItem data) : base(data)
        {
        }

        public string Recommendation
        {
            get
            {
                if (Data.Link == null || Data.Link.Scheme != StatusReportItem.HELIOS_SCHEME)
                {
                    // no link, we have nothing to edit
                    return Data.Recommendation;
                }

                if (string.IsNullOrEmpty(Data.Recommendation))
                {
                    // empty recommendation
                    return Data.Recommendation;
                }

                if (Data.Recommendation.Length < 2)
                {
                    // recommendation is too short to be a sentence, so avoid boundary case and do nothing
                    return Data.Recommendation;
                }

                string lowerCase;

                if (!char.IsUpper(Data.Recommendation[0]))
                {
                    // already starts wither lower case
                    lowerCase = Data.Recommendation;
                }
                else
                {
                    lowerCase = $"{char.ToLower(Data.Recommendation[0])}{Data.Recommendation.Substring(1)}";
                }

                switch (Data.Link.Host)
                {
                    case StatusReportItem.CONTROL_CENTER_HOST:
                        return $"Using Control Center, {lowerCase}";
                    case StatusReportItem.PROFILE_EDITOR_HOST:
                        return $"Using Profile Editor, {lowerCase}";
                    default:
                        return Data.Recommendation;
                }
            }
        }
    }
}