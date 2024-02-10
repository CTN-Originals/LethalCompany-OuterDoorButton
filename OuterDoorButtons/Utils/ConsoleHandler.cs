using UnityEngine;

namespace OuterDoorButtons.Utilities
{
	public static class Console {
		public static bool DebugState = false;

		public static void LogFatal(string message) 	{ SendLog(message, "LogFatal"); }
		public static void LogError(string message) 	{ SendLog(message, "LogError"); }
		public static void LogWarning(string message) 	{ SendLog(message, "LogWarning"); }
		public static void Log(string message) 			{ SendLog(message, "Log"); }
		public static void LogInfo(string message) 		{ SendLog(message, "LogInfo"); }
		public static void LogMessage(string message) 	{ SendLog(message, "LogMessage"); }
		public static void LogDebug(string message) 	{ SendLog(message, "LogDebug"); }

		private static void SendLog(string message, string level = null) {
			if (!DebugState && (level == "LogDebug" || level == "LogInfo")) return;

			switch(level) {
				case "LogFatal": 	Plugin.CLog.LogFatal(message); 		break;
				case "LogError": 	Plugin.CLog.LogError(message); 		break;
				case "LogWarning": 	Plugin.CLog.LogWarning(message);	break;
				case "LogInfo": 	Plugin.CLog.LogInfo(message); 		break;
				case "LogMessage": 	Plugin.CLog.LogMessage(message);	break;
				case "LogDebug": 	Plugin.CLog.LogDebug(message); 		break;
				default: {
					if (level != "Log") Debug.Log($"[{level}]: {message}");
					else Debug.Log(message);
				} break;
			}
		}
	}
}