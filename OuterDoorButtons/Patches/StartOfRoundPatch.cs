using HarmonyLib;
using UnityEngine;

using TMPro;
using GameNetcodeStuff;

using OuterDoorButtons.Utilities;
using UnityEngine.UI;

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
		// private static InteractTrigger CloseButtonTrigger;
		private static Transform PanelMeter;
		private static TextMeshProUGUI PanelMeterText;

		//? Trigger paths to turn of for better visibility (MeshRenderer.enabled = false)
		private static string[] VisibleTriggerPathsOrbit = {
			"Environment/HangarShip/ReverbTriggers/",
			"Environment/HangarShip/ReverbTriggers/LeavingShipTriggers/HorizontalTriggers/",
		};

		//? Trigger paths to turn of for better visibility (MeshRenderer.enabled = false)
		private static string[] VisibleTriggerPathsLanded = {
			"Environment/NavMeshColliders/PlayerShipNavmesh/",
			"Environment/ReverbTriggers (1)/WindTriggers/",
		};

		//? Path to a light to use for better lighting outside the ship for development
		//_ Environment/HangarShip/ShipModels2b/ShipLightsPost/Floodlight1

		[HarmonyPostfix]
		[HarmonyPatch("Start")]
		private static void StartPatch() {
			Console.LogInfo($"StartOfRound.Start() called");

			// //!! DEVELOPMENT ONLY
			// DisableVisibleTriggers(VisibleTriggerPathsOrbit);

			GetObjectReferences();
			if (MonitorWall == null || OriginalDoorPanel == null) return;
			CreateOuterDoorPanel();
		}

		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		private static void UpdatePatch() {
			if (OriginalPanelMeter == null || PanelMeterText == null) return;
			PanelMeterText.SetText(OriginalPanelMeter.text);
		}

		[HarmonyPostfix]
		[HarmonyPatch("OnShipLandedMiscEvents")]
		private static void OnShipLandedMiscEventsPatch() {
			Console.LogInfo($"StartOfRound.OnShipLandedMiscEvents() called");

			// //!! DEVELOPMENT ONLY
			// DisableVisibleTriggers(VisibleTriggerPathsLanded);
			// Transform originalLight = GameObject.Find("Environment/HangarShip/ShipModels2b/ShipLightsPost/Floodlight1").transform;
			// if (originalLight != null) {
			// 	Transform light = GameObject.Instantiate(originalLight, originalLight.parent);
			// 	light.name = "FloodlightDEV";
			// 	light.position = new Vector3(-14, 5, -15);
			// 	light.localEulerAngles = new Vector3(30, 0, 0);
			// 	Console.LogInfo($"StartOfRound.OnShipLandedMiscEvents() created: {light.name}");
			// }
		}

		//? Disable the MeshRenderer on the triggers for better visibility in the hangar for development
		private static void DisableVisibleTriggers(string[] triggerPaths) {
			foreach (string triggerPath in triggerPaths) {
				Transform trigger = GameObject.Find(triggerPath).transform;
				if (trigger == null) {
					Console.LogError($"StartOfRound.DisableTrigger() could not find: \n{triggerPath}");
					continue;
				}
				MeshRenderer renderer = trigger.GetComponent<MeshRenderer>();
				if (renderer != null) {
					renderer.enabled = false;
					Console.LogInfo($"Disabled {trigger.name}");
				}
				else {
					if (trigger.childCount > 0) {
						foreach (Transform child in trigger) {
							MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
							if (childRenderer != null) {
								childRenderer.enabled = false;
								Console.LogInfo($"Disabled {trigger.name} > {child.name}");
							}
						}
					}
					else Console.LogWarning($"StartOfRound.DisableTrigger() could not find MeshRenderer on {trigger.name}");
				}
			}
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
			//> Position (-6,36 2,15 -16,23)
				//- OpenButton
					//> Scale (-3,5 -3,5 -3,5)
			//> EulerAngles (90 90 0)
		private static void CreateOuterDoorPanel() {
			if (MonitorWall.Find("OuterDoorPanel") != null) {
				Console.LogError($"StartOfRound.CreateOuterDoorPanel() OuterDoorPanel already exists");
				return;
			}
			DoorPanel = GameObject.Instantiate(OriginalDoorPanel, OriginalDoorPanel.parent);
			DoorPanel.name = "OuterDoorPanel";
			DoorPanel.position = new Vector3(-6.36f, 2.15f, -16.23f);
			DoorPanel.localEulerAngles = new Vector3(90f, 90f, 0f);
			DoorPanel.GetComponent<MeshRenderer>().enabled = false;
			Console.LogInfo($"StartOfRound.CreateOuterDoorPanel() created: {DoorPanel.name}");

			//? Disable the scan node
			GameObject scanNode = DoorPanel.Find("ScanNode (2)").gameObject;
			if (scanNode != null) scanNode.SetActive(false);
			else Console.LogWarning($"StartOfRound.CreateOuterDoorPanel() could not find ScanNode (2)");

			//? Get the references for the new buttons
			OpenButton = DoorPanel.Find("StartButton").Find("Cube (2)");
			CloseButton = DoorPanel.Find("StopButton");
			if (OpenButton == null || CloseButton == null) {
				Console.LogError($"StartOfRound.CreateOuterDoorPanel() could not find OuterDoorPanel references");
				return;
			}
			CloseButton.gameObject.SetActive(false);

			OpenButton.parent.name = "OuterOpenButton";
			OpenButton.parent.localScale = new Vector3(-3.5f, -3.5f, -3.5f);

			// CloseButton.parent.name = "OuterCloseButton";
			// CloseButton.parent.localScale = new Vector3(-2f, -2f, -2f);

			OpenButtonTrigger = OpenButton.GetComponent<InteractTrigger>();
			// CloseButtonTrigger = CloseButton.GetComponent<InteractTrigger>();

			//+ Fix button animation and sound by by calling the right trigger on this object
			OpenButtonTrigger.onInteract.AddListener((player) => {
				CustomTrigger(player, OriginalOpenButtonTrigger, OpenButton, "Open");
			});
			// CloseButtonTrigger.onInteract.AddListener((player) => {
			// 	CustomTrigger(player, OriginalCloseButtonTrigger, CloseButton, "Close");
			// });

			//? find the meter text for later use in UpdatePatch()
			PanelMeter = DoorPanel.Find("ElevatorPanelScreen").transform;
			if (PanelMeter == null) { Console.LogError($"StartOfRound.CreateOuterDoorPanel() could not find ElevatorPanelScreen"); return; }
			PanelMeter.localPosition = new Vector3(0.1f, 0.02f, 0.32f);
			
			Transform PanelMeterImage = DoorPanel.Find("ElevatorPanelScreen/Image").transform;
			if (PanelMeterImage == null) { Console.LogError($"StartOfRound.CreateOuterDoorPanel() could not find Image on ElevatorPanelScreen"); return; }

			Image panelImage = PanelMeterImage.GetComponent<Image>();
			if (panelImage != null) panelImage.enabled = false;
			else Console.LogWarning($"StartOfRound.CreateOuterDoorPanel() could not find Image on ElevatorPanelScreen");

			GameObject hydra = PanelMeterImage.Find("doorHydraulics").gameObject;
			if (hydra != null) hydra.SetActive(false);
			else Console.LogWarning($"StartOfRound.CreateOuterDoorPanel() could not find doorHydraulics");

			PanelMeterText = PanelMeterImage.Find("meter").GetComponent<TextMeshProUGUI>();
			PanelMeterText.alignment = TextAlignmentOptions.Top;
			PanelMeterText.horizontalAlignment = HorizontalAlignmentOptions.Center;
		}

		private static void CustomTrigger(PlayerControllerB sender, InteractTrigger originalTrigger, Transform trigger, string state = "Open") {
			originalTrigger.onInteract.Invoke(sender);
			trigger.GetComponent<AnimatedObjectTrigger>().triggerAnimator.SetTrigger(state + "Door");
			Console.LogMessage($"StartOfRound.CustomTrigger() called for {trigger.name} with state {state}");
		}
	}
}