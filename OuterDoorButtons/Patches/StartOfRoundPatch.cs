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
		private static Transform DoorPanel;
		private static InteractTrigger DoorStartButtonTrigger;
		private static InteractTrigger DoorStopButtonTrigger;
		private static TextMeshProUGUI DoorPanelMeter;

		// The duplicate objects that are created when the round starts
		public static Transform MonitorDoorPanel;
		private static Transform MonitorStartButton;
		private static Transform MonitorStopButton;
		private static InteractTrigger MonitorStartButtonTrigger;
		private static InteractTrigger MonitorStopButtonTrigger;
		private static TextMeshProUGUI MonitorDoorPanelMeter;


		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		private static void StartPatch() {
			Console.LogInfo($"StartOfRound.Start() called");
			GetObjectReferences();
			if (MonitorWall == null || DoorPanel == null) return;
			CreateMonitorDoorPanel();
		}

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		private static void UpdatePatch() {
			if (DoorPanelMeter == null || MonitorDoorPanelMeter == null) return;
			MonitorDoorPanelMeter.SetText(DoorPanelMeter.text);
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
			
			DoorPanel = GameObject.Find("Environment/HangarShip/AnimatedShipDoor/HangarDoorButtonPanel").transform;
			Transform DoorStartButton = DoorPanel.Find("StartButton").Find("Cube (2)");
			Transform DoorStopButton = DoorPanel.Find("StopButton").Find("Cube (3)");
			if (DoorPanel == null || DoorStartButton == null || DoorStopButton == null) {
				Console.LogError($"StartOfRound.GetDoorPanel() could not find HangarDoorButtonPanel references");
				return;
			}
			DoorStartButtonTrigger = DoorStartButton.GetComponent<InteractTrigger>();
			DoorStopButtonTrigger = DoorStopButton.GetComponent<InteractTrigger>();
			DoorPanelMeter = DoorPanel.Find("ElevatorPanelScreen/Image/meter").GetComponent<TextMeshProUGUI>();

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
			MonitorDoorPanel = GameObject.Instantiate(DoorPanel, MonitorWall);
			MonitorDoorPanel.name = "MonitorDoorPanel";
			MonitorDoorPanel.localPosition = new Vector3(-0.2f, -1.7f, 0.15f);
			MonitorDoorPanel.localEulerAngles = new Vector3(90f, 90f, 0f);
			Console.LogInfo($"StartOfRound.CreateMonitorDoorPanel() created: {MonitorDoorPanel.name}");

			//? Get the references for the new buttons
			MonitorStartButton = MonitorDoorPanel.Find("StartButton").Find("Cube (2)");
			MonitorStopButton = MonitorDoorPanel.Find("StopButton").Find("Cube (3)");
			if (MonitorStartButton == null || MonitorStopButton == null) {
				Console.LogError($"StartOfRound.CreateMonitorDoorPanel() could not find MonitorDoorPanel references");
				return;
			}

			MonitorStartButtonTrigger = MonitorStartButton.GetComponent<InteractTrigger>();
			MonitorStopButtonTrigger = MonitorStopButton.GetComponent<InteractTrigger>();

			//+ Fix button animation and sound by by calling the right trigger on this object
			MonitorStartButtonTrigger.onInteract.AddListener((player) => {
				CustomTrigger(player, DoorStartButtonTrigger, MonitorStartButton, "Open");
			});
			MonitorStopButtonTrigger.onInteract.AddListener((player) => {
				CustomTrigger(player, DoorStopButtonTrigger, MonitorStopButton, "Close");
			});

			//? find the meter text for later use in UpdatePatch()
			MonitorDoorPanelMeter = MonitorDoorPanel.Find("ElevatorPanelScreen/Image/meter").GetComponent<TextMeshProUGUI>();
		}

		private static void CustomTrigger(PlayerControllerB sender, InteractTrigger originalTrigger, Transform trigger, string state = "Open") {
			originalTrigger.onInteract.Invoke(sender);
			trigger.GetComponent<AnimatedObjectTrigger>().triggerAnimator.SetTrigger(state + "Door");
			Console.LogMessage($"StartOfRound.CustomTrigger() called for {trigger.name} with state {state}");
		}
	}
}