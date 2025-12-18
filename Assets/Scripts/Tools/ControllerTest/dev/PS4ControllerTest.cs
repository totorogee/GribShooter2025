using UnityEngine;
using UnityEngine.InputSystem;

#if !ENABLE_INPUT_SYSTEM
#error "Input System is not enabled! Go to: Edit > Project Settings > Player > Active Input Handling and set it to 'Input System Package (New)' or 'Both'"
#endif

/// <summary>
/// Test script to check all PS4 controller buttons and inputs
/// Attach to any GameObject and press buttons to see debug output
/// </summary>
public class PS4ControllerTest : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Show button press messages in console")]
    public bool showButtonPresses = true;
    [Tooltip("Show continuous input values (sticks, triggers)")]
    public bool showContinuousInput = true;
    [Tooltip("Update interval for continuous input display")]
    public float updateInterval = 0.1f;
    [Tooltip("Show debug information about controller state")]
    public bool showDebugInfo = true;

    private Gamepad gamepad;
    private float lastUpdateTime = 0f;
    private float lastDebugTime = 0f;

    void Start()
    {
        // Verify Input System is available
        #if ENABLE_INPUT_SYSTEM
        Debug.Log("Input System is enabled");
        #else
        Debug.LogError("Input System is NOT enabled! Go to: Edit > Project Settings > Player > Active Input Handling");
        return;
        #endif
        
        // List all input devices
        Debug.Log("=== Checking Input Devices ===");
        var devices = InputSystem.devices;
        Debug.Log($"Total Input Devices: {devices.Count}");
        
        foreach (var device in devices)
        {
            Debug.Log($"- Device: {device.name} (Type: {device.GetType().Name}, Enabled: {device.enabled})");
        }
        
        // Find all gamepads
        var gamepads = Gamepad.all;
        Debug.Log($"\nTotal Gamepads: {gamepads.Count}");
        
        foreach (var gp in gamepads)
        {
            Debug.Log($"- Gamepad: {gp.name} (ID: {gp.deviceId}, Enabled: {gp.enabled})");
        }
        
        // Try to get current gamepad
        gamepad = Gamepad.current;
        
        if (gamepad == null && gamepads.Count > 0)
        {
            // Use first available gamepad if current is null
            gamepad = gamepads[0];
            Debug.Log($"Using first available gamepad: {gamepad.name}");
        }
        
        if (gamepad != null)
        {
            Debug.Log($"\n=== PS4 Controller Connected ===");
            Debug.Log($"Controller Name: {gamepad.name}");
            Debug.Log($"Device ID: {gamepad.deviceId}");
            Debug.Log($"Enabled: {gamepad.enabled}");
            Debug.Log($"Type: {gamepad.GetType().Name}");
            
            // Try to enable if not enabled
            if (!gamepad.enabled)
            {
                InputSystem.EnableDevice(gamepad);
                Debug.Log("Attempted to enable gamepad");
            }
            
            Debug.Log("Press any button or move sticks to test!");
            Debug.Log("========================================");
        }
        else
        {
            Debug.LogWarning("No gamepad detected! Please connect a controller.");
            Debug.LogWarning("Make sure Input System is enabled in Project Settings!");
        }
    }

    void Update()
    {
        // Try to reconnect if controller was disconnected
        if (gamepad == null)
        {
            gamepad = Gamepad.current;
            if (gamepad == null && Gamepad.all.Count > 0)
            {
                gamepad = Gamepad.all[0];
            }
            
            if (gamepad != null)
            {
                Debug.Log($"PS4 Controller reconnected: {gamepad.name}");
                if (!gamepad.enabled)
                {
                    InputSystem.EnableDevice(gamepad);
                }
            }
            return;
        }

        // Check if gamepad is still enabled
        if (!gamepad.enabled)
        {
            InputSystem.EnableDevice(gamepad);
            if (showDebugInfo)
            {
                Debug.LogWarning("Gamepad was disabled, attempting to re-enable...");
            }
        }

        // Test all buttons
        TestButtons();
        
        // Always test continuous input to see if anything works
        if (Time.time - lastUpdateTime > updateInterval)
        {
            TestContinuousInput();
            lastUpdateTime = Time.time;
        }
        
        // Periodic debug info
        if (showDebugInfo && Time.time - lastDebugTime > 2f)
        {
            ShowDebugInfo();
            lastDebugTime = Time.time;
        }
    }
    
    /// <summary>
    /// Show debug information about controller state
    /// </summary>
    private void ShowDebugInfo()
    {
        if (gamepad == null) return;
        
        // Check if any input is being received at all
        bool anyInput = false;
        
        // Check buttons
        if (gamepad.buttonSouth.isPressed || gamepad.buttonWest.isPressed || 
            gamepad.buttonNorth.isPressed || gamepad.buttonEast.isPressed ||
            gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed ||
            gamepad.leftStickButton.isPressed || gamepad.rightStickButton.isPressed ||
            gamepad.startButton.isPressed || gamepad.selectButton.isPressed)
        {
            anyInput = true;
        }
        
        // Check sticks
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        if (leftStick.magnitude > 0.01f || rightStick.magnitude > 0.01f)
        {
            anyInput = true;
        }
        
        // Check triggers
        if (gamepad.leftTrigger.ReadValue() > 0.01f || gamepad.rightTrigger.ReadValue() > 0.01f)
        {
            anyInput = true;
        }
        
        // Check D-pad
        if (gamepad.dpad.ReadValue().magnitude > 0.01f)
        {
            anyInput = true;
        }
        
        Debug.Log($"[Controller Debug] Enabled: {gamepad.enabled}, Any Input: {anyInput}, " +
                  $"Left Stick: {leftStick.magnitude:F3}, Right Stick: {rightStick.magnitude:F3}");
    }

    /// <summary>
    /// Test all PS4 controller buttons
    /// </summary>
    private void TestButtons()
    {
        // Face Buttons
        if (gamepad.buttonSouth.wasPressedThisFrame) // X Button
        {
            LogButton("X Button (South)", gamepad.buttonSouth.isPressed);
        }
        
        if (gamepad.buttonWest.wasPressedThisFrame) // Square Button
        {
            LogButton("Square Button (West)", gamepad.buttonWest.isPressed);
        }
        
        if (gamepad.buttonNorth.wasPressedThisFrame) // Triangle Button
        {
            LogButton("Triangle Button (North)", gamepad.buttonNorth.isPressed);
        }
        
        if (gamepad.buttonEast.wasPressedThisFrame) // Circle Button
        {
            LogButton("Circle Button (East)", gamepad.buttonEast.isPressed);
        }

        // Shoulder Buttons
        if (gamepad.leftShoulder.wasPressedThisFrame) // L1
        {
            LogButton("L1 (Left Shoulder)", gamepad.leftShoulder.isPressed);
        }
        
        if (gamepad.rightShoulder.wasPressedThisFrame) // R1
        {
            LogButton("R1 (Right Shoulder)", gamepad.rightShoulder.isPressed);
        }

        // Triggers (as buttons)
        if (gamepad.leftTrigger.wasPressedThisFrame) // L2
        {
            LogButton("L2 (Left Trigger)", gamepad.leftTrigger.isPressed);
        }
        
        if (gamepad.rightTrigger.wasPressedThisFrame) // R2
        {
            LogButton("R2 (Right Trigger)", gamepad.rightTrigger.isPressed);
        }

        // Stick Buttons
        if (gamepad.leftStickButton.wasPressedThisFrame) // L3
        {
            LogButton("L3 (Left Stick Press)", gamepad.leftStickButton.isPressed);
        }
        
        if (gamepad.rightStickButton.wasPressedThisFrame) // R3
        {
            LogButton("R3 (Right Stick Press)", gamepad.rightStickButton.isPressed);
        }

        // System Buttons
        if (gamepad.startButton.wasPressedThisFrame) // Options
        {
            LogButton("Options (Start)", gamepad.startButton.isPressed);
        }
        
        if (gamepad.selectButton.wasPressedThisFrame) // Share
        {
            LogButton("Share (Select)", gamepad.selectButton.isPressed);
        }

        // Touchpad (if available)
        if (gamepad is UnityEngine.InputSystem.DualShock.DualShockGamepad dualShock)
        {
            if (dualShock.touchpadButton.wasPressedThisFrame)
            {
                LogButton("Touchpad Button", dualShock.touchpadButton.isPressed);
            }
        }
    }

    /// <summary>
    /// Test continuous input (sticks, triggers, D-pad)
    /// </summary>
    private void TestContinuousInput()
    {
        if (gamepad == null) return;
        
        // Left Stick
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        if (showContinuousInput)
        {
            if (leftStick.magnitude > 0.1f)
            {
                Debug.Log($"Left Stick: X={leftStick.x:F2}, Y={leftStick.y:F2}, Magnitude={leftStick.magnitude:F2}");
            }
        }

        // Right Stick
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        if (showContinuousInput)
        {
            if (rightStick.magnitude > 0.1f)
            {
                Debug.Log($"Right Stick: X={rightStick.x:F2}, Y={rightStick.y:F2}, Magnitude={rightStick.magnitude:F2}");
            }
        }

        // Triggers
        float leftTrigger = gamepad.leftTrigger.ReadValue();
        float rightTrigger = gamepad.rightTrigger.ReadValue();
        if (showContinuousInput)
        {
            if (leftTrigger > 0.1f || rightTrigger > 0.1f)
            {
                Debug.Log($"Triggers: L2={leftTrigger:F2}, R2={rightTrigger:F2}");
            }
        }

        // D-Pad
        Vector2 dpad = gamepad.dpad.ReadValue();
        if (dpad.magnitude > 0.1f)
        {
            string dpadDirection = GetDPadDirection(dpad);
            Debug.Log($"🎮 D-Pad: {dpadDirection} (X={dpad.x:F2}, Y={dpad.y:F2})");
        }
        
        // Also check button states (not just presses)
        CheckButtonStates();
    }
    
    /// <summary>
    /// Check if buttons are currently pressed (not just pressed this frame)
    /// </summary>
    private void CheckButtonStates()
    {
        if (gamepad.buttonSouth.isPressed) Debug.Log("🎮 X Button is PRESSED");
        if (gamepad.buttonWest.isPressed) Debug.Log("🎮 Square Button is PRESSED");
        if (gamepad.buttonNorth.isPressed) Debug.Log("🎮 Triangle Button is PRESSED");
        if (gamepad.buttonEast.isPressed) Debug.Log("🎮 Circle Button is PRESSED");
        if (gamepad.leftShoulder.isPressed) Debug.Log("🎮 L1 is PRESSED");
        if (gamepad.rightShoulder.isPressed) Debug.Log("🎮 R1 is PRESSED");
        if (gamepad.leftStickButton.isPressed) Debug.Log("🎮 L3 is PRESSED");
        if (gamepad.rightStickButton.isPressed) Debug.Log("🎮 R3 is PRESSED");
        if (gamepad.startButton.isPressed) Debug.Log("🎮 Options is PRESSED");
        if (gamepad.selectButton.isPressed) Debug.Log("🎮 Share is PRESSED");
        
        // Check triggers as buttons
        if (gamepad.leftTrigger.ReadValue() > 0.5f) Debug.Log($"🎮 L2 is PRESSED ({gamepad.leftTrigger.ReadValue():F2})");
        if (gamepad.rightTrigger.ReadValue() > 0.5f) Debug.Log($"🎮 R2 is PRESSED ({gamepad.rightTrigger.ReadValue():F2})");
    }

    /// <summary>
    /// Manual test - call from context menu
    /// </summary>
    [ContextMenu("Test Controller Input")]
    public void ManualTest()
    {
        Debug.Log("=== MANUAL CONTROLLER TEST ===");
        
        var gamepads = Gamepad.all;
        Debug.Log($"Found {gamepads.Count} gamepad(s)");
        
        if (gamepads.Count == 0)
        {
            Debug.LogError("No gamepads found! Check:");
            Debug.LogError("1. Controller is connected (USB or Bluetooth)");
            Debug.LogError("2. Input System is enabled: Edit > Project Settings > Player > Active Input Handling");
            Debug.LogError("3. Input System Package is installed: Window > Package Manager > Input System");
            return;
        }
        
        foreach (var gp in gamepads)
        {
            Debug.Log($"\n--- Testing: {gp.name} ---");
            Debug.Log($"Enabled: {gp.enabled}");
            Debug.Log($"Device ID: {gp.deviceId}");
            
            if (!gp.enabled)
            {
                InputSystem.EnableDevice(gp);
                Debug.Log("Attempted to enable device");
            }
            
            // Test reading values
            Vector2 leftStick = gp.leftStick.ReadValue();
            Vector2 rightStick = gp.rightStick.ReadValue();
            float leftTrigger = gp.leftTrigger.ReadValue();
            float rightTrigger = gp.rightTrigger.ReadValue();
            
            Debug.Log($"Left Stick: {leftStick}");
            Debug.Log($"Right Stick: {rightStick}");
            Debug.Log($"L2: {leftTrigger}, R2: {rightTrigger}");
            Debug.Log($"X Button: {gp.buttonSouth.isPressed}");
            Debug.Log($"Square Button: {gp.buttonWest.isPressed}");
        }
        
        Debug.Log("=============================");
    }

    /// <summary>
    /// Log button press with status
    /// </summary>
    private void LogButton(string buttonName, bool isPressed)
    {
        if (showButtonPresses)
        {
            string status = isPressed ? "PRESSED" : "RELEASED";
            Debug.Log($"🎮 [{status}] {buttonName}");
        }
    }

    /// <summary>
    /// Get D-pad direction as string
    /// </summary>
    private string GetDPadDirection(Vector2 dpad)
    {
        if (dpad.y > 0.5f) return "UP";
        if (dpad.y < -0.5f) return "DOWN";
        if (dpad.x > 0.5f) return "RIGHT";
        if (dpad.x < -0.5f) return "LEFT";
        if (dpad.magnitude > 0.1f) return "DIAGONAL";
        return "CENTER";
    }

    /// <summary>
    /// Display controller status in OnGUI (optional visual feedback)
    /// </summary>
    void OnGUI()
    {
        if (gamepad == null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), "No PS4 Controller Detected");
            return;
        }

        GUI.Label(new Rect(10, 10, 300, 20), $"PS4 Controller: {gamepad.name}");
        GUI.Label(new Rect(10, 30, 300, 20), "Press buttons to test!");
        
        // Show current stick positions
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        
        GUI.Label(new Rect(10, 50, 300, 20), $"Left Stick: ({leftStick.x:F2}, {leftStick.y:F2})");
        GUI.Label(new Rect(10, 70, 300, 20), $"Right Stick: ({rightStick.x:F2}, {rightStick.y:F2})");
        
        // Show trigger values
        float leftTrigger = gamepad.leftTrigger.ReadValue();
        float rightTrigger = gamepad.rightTrigger.ReadValue();
        GUI.Label(new Rect(10, 90, 300, 20), $"L2: {leftTrigger:F2} | R2: {rightTrigger:F2}");
    }
}
