using System;
using System.Collections.Generic;

namespace TextUITemplate.Management
{
    public class Button
    {
        public string title { get; set; }
        public string tooltip { get; set; }
        public bool toggled { get; set; }
        public bool isToggleable { get; set; }
        public Action action { get; set; }
        public Action disableAction { get; set; }

        public List<AdjustableValue> ModAdjustableValues { get; set; } = new List<AdjustableValue>();
        public bool HasAdjustableValues => ModAdjustableValues != null && ModAdjustableValues.Count > 0;

        public List<ModPreset> Presets { get; set; } = new List<ModPreset>();
        public bool HasPresets => Presets != null && Presets.Count > 0;
    }
}