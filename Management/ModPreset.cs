using System.Collections.Generic;

namespace TextUITemplate.Management
{
    public class ModPreset
    {
        public string Name { get; set; }
        public Dictionary<string, object> Values { get; set; }

        public ModPreset()
        {
            Values = new Dictionary<string, object>();
        }
    }
}