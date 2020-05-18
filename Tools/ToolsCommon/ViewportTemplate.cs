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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace ToolsCommon
{
    /// <summary>
    /// This is a loader for Jabbers' JSON format for viewport templates.  It must be kep compatible with
    /// https://github.com/jeffboulanger/DCS-Alternative-Launcher/blob/master/DCS.Alternative.Launcher/DomainObjects/ModuleViewportTemplate.cs
    /// </summary>
    public class ViewportTemplate
    {
        private static readonly Dictionary<string, string> _moduleCategories = new Dictionary<string, string>
        {
            // make these fit in with template / control categories in Helios UI
            {"F-16C", "F-16"},
            {"FA-18C", "F/A-18C"},
            {"HEATBLUR_F-14", "F-14"},
            {"M-2000C", "M2000C"},
            {"Mi-8MTV2", "Mi-8"},
            {"MIG-21bis", "MiG-21"}
        };

        [JsonProperty("templateName")] public string TemplateName { get; set; }

        [JsonProperty("isHeliosTemplate")] public bool IsHeliosTemplate { get; set; }

        [JsonProperty("moduleId")] public string ModuleId { get; set; }

        [JsonProperty("viewports")] public List<Viewport> Viewports { get; set; } = new List<Viewport>();

        // XXX eliminate this in favor of viewport prefix from file
        [JsonIgnore] public string ViewportPrefix => ModuleId.Replace(" ", "_").Replace("-", "_");

        [JsonIgnore]
        public object TemplateCategory
        {
            get
            {
                if (_moduleCategories.TryGetValue(ModuleId, out string category))
                {
                    return category;
                }

                return ModuleId;
            }
        }

        /// <summary>
        /// stable template name to use in Helios UI
        /// </summary>
        public string TemplateDisplayName =>
            TemplateName.Replace("-", "").Replace("Helios ", "").Replace(" (16/9)", "");

        /// <summary>
        /// stale viewport name to use in Helios UI
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public string DisplayName(Viewport viewport) =>
            $"{TemplateDisplayName} {(viewport.Description ?? viewport.ViewportName).Replace("-", " ")}";
    }
}