using HarmonyLib;
using UnityEngine;

using TMPro;
using GameNetcodeStuff;

using OuterDoorButtons.Utilities;

namespace OuterDoorButtons.Patches
{
	[HarmonyPatch(typeof(StartOfRound))]
	internal class StartOfRoundPatch {
		// The original objects as a reference
		private static Transform MonitorWall;
		private static Transform OriginalDoorPanel;
		private static InteractTrigger OriginalOpenButtonTrigger;
		private static InteractTrigger OriginalCloseButtonTrigger;
		private static TextMeshProUGUI OriginalPanelMeter;

		// The duplicate objects that are created when the round starts
		public static Transform DoorPanel;
		private static Transform OpenButton;
		private static Transform CloseButton;
		private static InteractTrigger OpenButtonTrigger;
		private static InteractTrigger CloseButtonTrigger;
		private static TextMeshProUGUI PanelMeter;


		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		private static void StartPatch() {
			Console.LogInfo($"StartOfRound.Start() called");
			GetObjectReferences();
			if (MonitorWall == null || OriginalDoorPanel == null) return;
			CreateMonitorDoorPanel();
		}

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		private static void UpdatePatch() {
			if (OriginalPanelMeter == null || PanelMeter == null) return;
			PanelMeter.SetText(OriginalPanelMeter.text);
		}

		//_ Monitor Camera buttons: Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001/
			//- CameraMonitorOnButton
				//> Cube (2)
			//- CameraMonitorSwitchButton
				//> Cube (2)
		//_ Door open/close panel: Environment/HangarShip/AnimatedShipDoor/HangarDoorButtonPanel/
			//- StartButton
				//> Cube (2)
				//() AnimatedObjectTrigger.SetTrigger("OpenDoor")
			//- StopButton
				//> Cube (3)
				//() AnimatedObjectTrigger.SetTrigger("CloseDoor")
			//- ElevatorPanelScreen/Image/meter
				//() TextMeshProUGUI
		private static void GetObjectReferences() {
			MonitorWall = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall").transform;
			if (MonitorWall == null) {
				Console.LogError($"StartOfRound.GetMonitorButton() could not find MonitorWall");
				return;
			}
			
			OriginalDoorPanel = GameObject.Find("Environment/HangarShip/AnimatedShipDoor/HangarDoorButtonPanel").transform;
			Transform DoorStartButton = OriginalDoorPanel.Find("StartButton").Find("Cube (2)");
			Transform DoorStopButton = OriginalDoorPanel.Find("StopButton").Find("Cube (3)");
			if (OriginalDoorPanel == null || DoorStartButton == null || DoorStopButton == null) {
				Console.LogError($"StartOfRound.GetDoorPanel() could not find HangarDoorButtonPanel references");
				return;
			}
			OriginalOpenButtonTrigger = DoorStartButton.GetComponent<InteractTrigger>();
			OriginalCloseButtonTrigger = DoorStopButton.GetComponent<InteractTrigger>();
			OriginalPanelMeter = OriginalDoorPanel.Find("ElevatorPanelScreen/Image/meter").GetComponent<TextMeshProUGUI>();

			// Plugin.Console.LogInfo($"StartOfRound.GetDoorPanel() found HangarDoorButtonPanel: {DoorPanel.name}");
			// Plugin.Console.LogInfo($"StartOfRound.GetMonitorButton() found MonitorWall: {MonitorWall.name}");
		}

		//| Transform
			//> LocalPosition (-0,2 -1,7 0,15)
			//> EulerAngles (90 90 0)
		private static void CreateMonitorDoorPanel() {
			if (MonitorWall.Find("MonitorDoorPanel") != null) {
				Console.LogError($"StartOfRound.CreateMonitorDoorPanel() MonitorDoorPanel already exists");
				return;
			}
			DoorPanel = GameObject.Instantiate(OriginalDoorPanel, MonitorWall);
			DoorPanel.name = "MonitorDoorPanel";
			DoorPanel.localPosition = new Vector3(-0.2f, -1.7f, 0.15f);
			DoorPanel.localEulerAngles = new Vector3(90f, 90f, 0f);
			Console.LogInfo($"StartOfRound.CreateMonitorDoorPanel() created: {DoorPanel.name}");

			//? Get the references for the new buttons
			OpenButton = DoorPanel.Find("StartButton").Find("Cube (2)");
			CloseButton = DoorPanel.Find("StopButton").Find("Cube (3)");
			if (OpenButton == null || CloseButton == null) {
				Console.LogError($"StartOfRound.CreateMonitorDoorPanel() could not find MonitorDoorPanel references");
				return;
			}

			OpenButtonTrigger = OpenButton.GetComponent<InteractTrigger>();
			CloseButtonTrigger = CloseButton.GetComponent<InteractTrigger>();

			//+ Fix button animation and sound by by calling the right trigger on this object
			OpenButtonTrigger.onInteract.AddListener((player) => {
				CustomTrigger(player, OriginalOpenButtonTrigger, OpenButton, "Open");
			});
			CloseButtonTrigger.onInteract.AddListener((player) => {
				CustomTrigger(player, OriginalCloseButtonTrigger, CloseButton, "Close");
			});

			//? find the meter text for later use in UpdatePatch()
			PanelMeter = DoorPanel.Find("ElevatorPanelScreen/Image/meter").GetComponent<TextMeshProUGUI>();
		}

		private static void CustomTrigger(PlayerControllerB sender, InteractTrigger originalTrigger, Transform trigger, string state = "Open") {
			originalTrigger.onInteract.Invoke(sender);
			trigger.GetComponent<AnimatedObjectTrigger>().triggerAnimator.SetTrigger(state + "Door");
			Console.LogMessage($"StartOfRound.CustomTrigger() called for {trigger.name} with state {state}");
		}
	}
}