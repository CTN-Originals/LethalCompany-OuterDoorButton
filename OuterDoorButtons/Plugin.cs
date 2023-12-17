using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace OuterDoorButtons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
		private static readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
		public static ManualLogSource CLog;

        private void Awake() {
            CLog = Logger;
			CLog.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
			harmony.PatchAll();
        }
    }
}
