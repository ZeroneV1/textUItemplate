using System.Collections.Generic;

namespace TextUITemplate.Management
{
    public class ModConfigState
    {
        public string ModTitle { get; set; }
        public bool IsEnabled { get; set; }
        public Dictionary<string, object> AdjustableValues { get; set; }

        public ModConfigState()
        {
            AdjustableValues = new Dictionary<string, object>();
        }
    }

    public class AutoConfigData
    {
        public List<ModConfigState> ModStates { get; set; }

        public AutoConfigData()
        {
            ModStates = new List<ModConfigState>();
        }
    }
}