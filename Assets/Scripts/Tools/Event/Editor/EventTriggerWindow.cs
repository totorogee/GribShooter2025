using UnityEngine;
using System.Linq;
using UnityEditor;
using System;
using System.Collections.Generic;
using GameTool.Hex;

namespace GameTool
{

#if UNITY_EDITOR

    [System.Serializable]
    public class ArgumentInput
    {
        public Type InputType;
        public dynamic InputDynamic;
        public string SerializedName;

        // Custom Class as Accepted Type
        [SerializeField] public JsonDataTest JsonDataTest = new JsonDataTest();
        [SerializeField] public Direction9 Direction9 = new Direction9();
        [SerializeField] public HexInt HexInt = new HexInt();
    }

    public class EventTriggerWindow : EditorWindow
    {
        // Constants
        private const string WINDOW_TITLE = "Event Trigger";
        private const string NOT_IN_PLAY_MODE_MESSAGE = "Not in Play Mode";
        private const string NOT_SUPPORTED_MESSAGE = "Not Support";
        private const KeyCode TRIGGER_HOTKEY = KeyCode.E;

        [SerializeField] public ArgumentInput Arg1;
        [SerializeField] public ArgumentInput Arg2;

        private string[] eventOptions = { "" };
        private int eventOptionsIndex;

        private string[] eventNames = { "" };
        
        // Debug and refresh controls
        private bool showDebugInfo = true;
        private bool forceRefresh = false;
        private int lastRegisteredEventCount = 0;
        private bool hideInfoSection = false;

        private SerializedObject serializedObject;
        private Action triggerEvent;
        private bool needsEventDataUpdate = true;
        private float lastEventDataUpdateTime = 0f;
        private const float EVENT_DATA_UPDATE_INTERVAL = 0.1f; // Update every 100ms max

        [MenuItem("GameTool/Event Triggering Window")]
        public static void Init()
        {
            EventTriggerWindow window = (EventTriggerWindow) GetWindow(typeof(EventTriggerWindow) , false , WINDOW_TITLE);
            window.Show();
            window.Arg1 = new ArgumentInput { SerializedName = nameof(window.Arg1) };
            window.Arg2 = new ArgumentInput { SerializedName = nameof(window.Arg2) };
        }

        private Dictionary<Type, Action<ArgumentInput>> GetInputField;

        private void InitializeInputFields()
        {
            if (GetInputField == null)
            {
                GetInputField = new Dictionary<Type, Action<ArgumentInput>>
                {
                    { typeof(int) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is int)) { arg.InputDynamic = 0; }
                        arg.InputDynamic = EditorGUILayout.IntField(arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(float) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is float)) { arg.InputDynamic = 0f; }
                        arg.InputDynamic = EditorGUILayout.FloatField(arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(bool) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is bool)) { arg.InputDynamic = true; }
                        arg.InputDynamic = EditorGUILayout.Toggle(arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(string) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is string)) { arg.InputDynamic = string.Empty; }
                        arg.InputDynamic = EditorGUILayout.TextField (arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(Vector2) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is Vector2)) { arg.InputDynamic = new Vector2(); }
                        arg.InputDynamic = EditorGUILayout.Vector2Field(arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(Vector2Int) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is Vector2Int)) { arg.InputDynamic = new Vector2Int(); }
                        arg.InputDynamic = EditorGUILayout.Vector2IntField(arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(Vector3) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is Vector3)) { arg.InputDynamic = new Vector3(); }
                        arg.InputDynamic = EditorGUILayout.Vector3Field(arg.InputType.Name, arg.InputDynamic);
                    }},

                    { typeof(Vector3Int) , (arg)=> {
                        if (arg.InputDynamic == null || !(arg.InputDynamic is Vector3Int)) { arg.InputDynamic = new Vector3Int(); }
                        arg.InputDynamic = EditorGUILayout.Vector3IntField(arg.InputType.Name, arg.InputDynamic);
                    }},

                    // Custom Class as Accepted Type
                    { typeof(JsonDataTest) , (arg)=> {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(arg.SerializedName).FindPropertyRelative(nameof(arg.JsonDataTest)), true);
                        arg.InputDynamic = arg.JsonDataTest;
                    }},

                    { typeof(Direction9) , (arg)=> {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(arg.SerializedName).FindPropertyRelative(nameof(arg.Direction9)), true);
                        arg.InputDynamic = arg.Direction9;
                    }},

                    { typeof(HexInt) , (arg)=> {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(arg.SerializedName).FindPropertyRelative(nameof(arg.HexInt)), true);
                        arg.InputDynamic = arg.HexInt;
                    }},

                };
            }
        }


        void InputArgument(ArgumentInput arg)
        {
            if (arg.InputType == null) { return; }

            // Initialize input fields if not already done
            InitializeInputFields();

            if (GetInputField.ContainsKey(arg.InputType))
            {
                GetInputField[arg.InputType](arg);
            }
            else
            {
                arg.InputType = null;
                EditorGUILayout.LabelField(NOT_SUPPORTED_MESSAGE);
            }
        }

        void OnGUI()
        {
            // Only create SerializedObject if needed or if it's null
            if (serializedObject == null || serializedObject.targetObject != this)
            {
                serializedObject = new SerializedObject(this);
            }

            if (EditorApplication.isPlaying)
            {
                // Update event data only when necessary
                UpdateEventDataIfNeeded();

                // === INPUT CONTROLS SECTION (TOP) ===
                EditorGUILayout.LabelField("Event Controls", EditorStyles.boldLabel);
                
                if (eventOptions.Length > 0)
                {
                    eventOptionsIndex = EditorGUILayout.Popup("Event Name", eventOptionsIndex, eventOptions);
                    InputArgument(Arg1);
                    InputArgument(Arg2);

                    UpdateTriggerEvent();
                    DrawActionButtons();
                }
                else
                {
                    EditorGUILayout.HelpBox("No events registered. Make sure your scripts are running and have subscribed to events.", MessageType.Warning);
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Refresh Events"))
                    {
                        forceRefresh = true;
                        needsEventDataUpdate = true;
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.Separator();
                EditorGUILayout.Space();

                // === DEBUG CONTROLS ===
                EditorGUILayout.BeginHorizontal();
                showDebugInfo = EditorGUILayout.Toggle("Show Debug Info", showDebugInfo);
                hideInfoSection = EditorGUILayout.Toggle("Hide Info Section", hideInfoSection);
                if (GUILayout.Button("Force Refresh", GUILayout.Width(100)))
                {
                    forceRefresh = true;
                    needsEventDataUpdate = true;
                }
                EditorGUILayout.EndHorizontal();

                // === DEBUG INFORMATION SECTION (BOTTOM) ===
                if (showDebugInfo && !hideInfoSection)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Registered Events: {lastRegisteredEventCount}");
                    EditorGUILayout.LabelField($"Event Options Count: {eventOptions.Length}");
                    EditorGUILayout.LabelField($"Current Selection: {eventOptionsIndex}");
                    
                    if (eventOptions.Length > 0)
                    {
                        EditorGUILayout.LabelField("Available Events:");
                        for (int i = 0; i < eventOptions.Length; i++)
                        {
                            EditorGUILayout.LabelField($"  {i}: {eventOptions[i]}");
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(NOT_IN_PLAY_MODE_MESSAGE);
            }

            // Only apply if we have changes
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void Update()
        {
            if (hasFocus && EditorApplication.isPlaying) 
            { 
                // Only update if enough time has passed
                if (Time.time - lastEventDataUpdateTime > EVENT_DATA_UPDATE_INTERVAL)
                {
                    UpdateEventData();
                    lastEventDataUpdateTime = Time.time;
                }
            }

            if (Input.GetKeyDown(TRIGGER_HOTKEY)) { OnTriggerPressed(); }
        }

        private void OnFocus()
        {
            // Force refresh when window gains focus
            if (EditorApplication.isPlaying)
            {
                forceRefresh = true;
                needsEventDataUpdate = true;
            }
        }

        private void UpdateEventDataIfNeeded()
        {
            if (needsEventDataUpdate || forceRefresh)
            {
                UpdateEventData();
                needsEventDataUpdate = false;
                forceRefresh = false;
            }
        }

        private void UpdateEventData()
        {
            var RegisteredEvents = EventManager.GetRegistoredEvents().ToList();
            lastRegisteredEventCount = RegisteredEvents.Count;

            if (RegisteredEvents.Count > 0)
            {
                var newEventOptions = RegisteredEvents.Select(item => item.Key.ToString() ).ToArray();
                var newEventNames = RegisteredEvents.Select(item => item.Key.EventName).ToArray();
                
                // Always update when force refresh is requested
                if (forceRefresh || !AreArraysEqual(eventOptions, newEventOptions) || !AreArraysEqual(eventNames, newEventNames))
                {
                    eventOptions = newEventOptions;
                    eventNames = newEventNames;
                    needsEventDataUpdate = true; // Mark for next OnGUI update
                    
                    // Reset selection if it's out of bounds
                    if (eventOptionsIndex >= eventOptions.Length)
                    {
                        eventOptionsIndex = 0;
                    }
                }
            }
            else
            {
                // No events registered
                eventOptions = new string[] { "" };
                eventNames = new string[] { "" };
                eventOptionsIndex = 0;
            }

            if (eventOptionsIndex < RegisteredEvents.Count && RegisteredEvents.Count > 0)
            {
                Arg1.InputType = RegisteredEvents[eventOptionsIndex].Key.ParamType1;
                Arg2.InputType = RegisteredEvents[eventOptionsIndex].Key.ParamType2;
            }
            else
            {
                Arg1.InputType = null;
                Arg2.InputType = null;
            }
        }

        private bool AreArraysEqual(string[] array1, string[] array2)
        {
            if (array1 == null && array2 == null) return true;
            if (array1 == null || array2 == null) return false;
            if (array1.Length != array2.Length) return false;
            
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }

        private void UpdateTriggerEvent()
        {
            if (IsValidEventIndex())
            {
                if (Arg2.InputType == null && Arg1.InputType == null)
                {
                    triggerEvent = () => EventManager.TriggerEvent(eventNames[eventOptionsIndex]);
                }
                else if (Arg2.InputType == null)
                {
                    // Handle single parameter events
                    if (Arg1.InputType == typeof(string))
                    {
                        triggerEvent = () => EventManager.TriggerEvent<string>(eventNames[eventOptionsIndex], (string)Arg1.InputDynamic);
                    }
                    else if (Arg1.InputType == typeof(int))
                    {
                        triggerEvent = () => EventManager.TriggerEvent<int>(eventNames[eventOptionsIndex], (int)Arg1.InputDynamic);
                    }
                    else if (Arg1.InputType == typeof(float))
                    {
                        triggerEvent = () => EventManager.TriggerEvent<float>(eventNames[eventOptionsIndex], (float)Arg1.InputDynamic);
                    }
                    else if (Arg1.InputType == typeof(bool))
                    {
                        triggerEvent = () => EventManager.TriggerEvent<bool>(eventNames[eventOptionsIndex], (bool)Arg1.InputDynamic);
                    }
                    else
                    {
                        triggerEvent = () => EventManager.TriggerEvent(eventNames[eventOptionsIndex], Arg1.InputDynamic);
                    }
                }
                else
                {
                    triggerEvent = () => EventManager.TriggerEvent(eventNames[eventOptionsIndex], Arg1.InputDynamic, Arg2.InputDynamic);
                }
            }
            else
            {
                triggerEvent = null;
            }
        }

        private void DrawActionButtons()
        {
            if (GUILayout.Button("Check")) OnCheckPressed();
            if (GUILayout.Button("Trigger")) OnTriggerPressed();
        }

        // Button Pressed 
        private void OnCheckPressed()
        {
            if (eventNames.Length > eventOptionsIndex)
            {
                Debug.LogWarning(eventNames[eventOptionsIndex] + " " + Arg1.InputType?.FullName + " " + Arg2.InputType?.FullName);
            }
        }

        private void OnTriggerPressed()
        {
            if (IsValidEventIndex() && triggerEvent != null) 
            { 
                try
                {
                    triggerEvent(); 
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to trigger event '{eventNames[eventOptionsIndex]}': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Cannot trigger event: Invalid event index or null trigger action");
            }
        }

        private bool IsValidEventIndex()
        {
            return eventNames != null && eventNames.Length > 0 && 
                   eventOptionsIndex >= 0 && eventOptionsIndex < eventNames.Length;
        }
    }
#endif
}
