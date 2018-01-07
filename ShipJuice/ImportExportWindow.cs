using Smooth.Algebraics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShipJuice
{
    [KSPAddon (KSPAddon.Startup.Flight, false)]
    public class ImportExportWindow : MonoBehaviour
    {
        private static ImportExportWindow Instance;
        private static bool HideUI = false;
        private static bool GuiEnabled = false;
        private static Rect WindowPos = new Rect(200,200,600,400);
        private static GUIStyle On;
        private static GUIStyle Off;
        private static GUIStyle Close;
        private static GUIStyle Right;
        private static ModuleImportDock Dock;

        public static void ToggleGUI()
        {
            GuiEnabled = !GuiEnabled;
            if (Instance != null)
            {
                Instance.UpdateGUIState();
            }
        }

        public static void HideGUI()
        {
            GuiEnabled = false;
            if (Instance != null)
            {
                Instance.UpdateGUIState();
            }
        }

        public static void ShowGUI(ModuleImportDock dock)
        {
            Dock = dock;

            GuiEnabled = true;
            if (Instance != null)
            {
                Instance.UpdateGUIState();
            }
        }
        
        void UpdateGUIState()
        {
            enabled = !HideUI && GuiEnabled;

            // build ui
        }

        void onHideUI()
        {
            HideUI = true;
            UpdateGUIState();
        }

        void onShowUI()
        {
            HideUI = false;
            UpdateGUIState();
        }

        void Awake()
        {
            Instance = this;
            GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
            enabled = false;
        }

        void OnDestroy()
        {
            Instance = null;
            GameEvents.onVesselChange.Remove(onVesselChange);
            GameEvents.onVesselWasModified.Remove(onVesselWasModified);
            GameEvents.onHideUI.Remove(onHideUI);
            GameEvents.onShowUI.Remove(onShowUI);
        }


        void onVesselChange(Vessel v)
        {
            //CurrentVessel = v;
            UpdateGUIState();
        }

        void onVesselWasModified(Vessel v)
        {
            if (FlightGlobals.ActiveVessel == v)
            {
                //CurrentVessel = v;
                UpdateGUIState();
            }
        }


        private Vector2 scrollPosition = Vector2.zero;

        void WindowGUI(int windowID)
        {
            //Styles.Init();

            GUILayout.BeginHorizontal(Right, GUILayout.Width(600));
            if(GUILayout.Button(new GUIContent("x"), Close, GUILayout.Width(50)))
            {
                Dock.FlipMenus(false);
                HideGUI();
            }
            GUILayout.EndHorizontal();

            var sjm = ShipJuiceManager.Instance;

            if (sjm == null)
            {
                return;
            }

            var resources = sjm.includedResources;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(600), GUILayout.Height(400));
            GUILayout.BeginVertical();

            foreach(var resName in resources.Keys)
            {
                var res = resources[resName];
                var disabled = sjm.disabledResources.Contains(new Tuple<string,uint>(resName, Dock.GetCraftId()));

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(disabled ? new GUIContent("Off") : new GUIContent("On"), disabled ? Off : On, GUILayout.Width(50)))
                {
                    Dock.ToggleResource(resName);
                }

                GUILayout.Label(new GUIContent(res.displayName), GUILayout.Width(550));
                
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();


            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
        
        void OnGUI()
        {
            if(On == null)
            {
                On = new GUIStyle(GUI.skin.button);
                On.normal.textColor = Color.green;

                Off = new GUIStyle(GUI.skin.button);
                Off.normal.textColor = Color.white;

                Close = new GUIStyle(GUI.skin.button);
                Close.normal.textColor = Color.red;
                Close.alignment = TextAnchor.MiddleCenter;

                Right = new GUIStyle(GUI.skin.box);
                Right.alignment = TextAnchor.UpperRight;
            }

            GUI.skin = HighLogic.Skin;

            string name = "Edit Import/Export";
            string ver = "0.1b";

            string sit = Dock == null ? "no vessel" : Dock.vessel.situation.ToString();

            WindowPos = GUILayout.Window(GetInstanceID(),
                                          WindowPos, WindowGUI,
                                          name + " " + ver + ": " + sit,
                                          GUILayout.Width(695));
        }
    }
}
