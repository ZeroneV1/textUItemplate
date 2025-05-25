// Path: TextUITemplate/Management/Button.cs
using System;
using System.Collections.Generic; // Required for List
using TextUITemplate.Mods;       // Required for AdjustableValue

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

        // AdjustableValues, HasAdjustableValues, and OnValuesChangedAction are removed
    }
}