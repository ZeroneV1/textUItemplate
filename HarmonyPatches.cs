using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TextUITemplate
{
    public class HarmonyPatches
    {
        public static Harmony instance;
        public static bool patched { get; private set; }
        public const string id = PluginInfo.GUID;

        public static void Patch(bool toggle)
        {
            if (toggle)
            {
                if (!patched)
                {
                    if (instance == null)
                        instance = new Harmony(id);
                }

                instance.PatchAll(Assembly.GetExecutingAssembly());
                patched = true;
            }
            else
            {
                if (instance != null)
                {
                    if (patched)
                        instance.UnpatchSelf();
                }
                patched = false;
            }
        }
    }
}
