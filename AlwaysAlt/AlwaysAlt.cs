using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;
using UnityEngine.UI;
using UnityEngine;

namespace AlwaysAlt
{
#if DEBUG
    [EnableReloading]
#endif
    public static class AlwaysAlt
    {
        private static Harmony _hi;
        public static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
        public static bool Enabled { get; set; } = true;

        public static void Log(string message) => Logger?.Log(message);

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            _hi = new Harmony(modEntry.Info.Id);
            _hi.PatchAll(Assembly.GetExecutingAssembly());
            Log($"Patched {string.Join(", ", _hi.GetPatchedMethods().Select(mb => $"{mb.DeclaringType}.{mb.Name}"))}");
#if DEBUG
            modEntry.OnUnload = Unload;
#endif
            modEntry.OnToggle = Toggle;
            return true;
        }

#if DEBUG
        public static bool Unload(UnityModManager.ModEntry modEntry)
        {
            Log($"Unpatching");
            _hi.UnpatchAll();
            return true;
        }
#endif

        public static bool Toggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }
    }

    [HarmonyPatch]
    internal class TooltipPatches
    {
        [HarmonyPrefix, HarmonyPatch(typeof(TooltipPanel), "SetupFeatures")]
        public static void Prefix_TooltipPanel_SetupFeatures(ref TooltipDefinitions.Scope scope, GuiTooltipClassDefinition tooltipClassDefinition, Dictionary<string, TooltipFeature> tooltipsFeatures)
        {
            if (AlwaysAlt.Enabled)
            {
                if (scope == TooltipDefinitions.Scope.Simplified)
                    scope = TooltipDefinitions.Scope.Detailed;
                else if (scope == TooltipDefinitions.Scope.Detailed)
                    scope = TooltipDefinitions.Scope.Simplified;
            }
        }
    }
}
