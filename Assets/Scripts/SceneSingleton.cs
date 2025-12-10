using UnityEngine;

/// <summary>
/// Generic singleton pattern for MonoBehaviour components that exist only in the current scene.
/// Ensures only one instance exists per scene but doesn't recreate on scene changes.
/// 
/// Usage:
/// 1. Create a class that inherits from SceneSingleton<YourClass>
/// 2. Access the instance using YourClass.Instance
/// 
/// Example:
/// public class GameManager : SceneSingleton<GameManager>
/// {
///     public void DoSomething() { ... }
/// }
/// 
/// // Usage elsewhere:
/// GameManager.Instance.DoSomething();
/// </summary>
/// <typeparam name="T">The MonoBehaviour type that will be made singleton</typeparam>
public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>The singleton instance</summary>
    private static T _instance;

    /// <summary>Thread safety lock for instance creation</summary>
    private static object _lock = new object();

    /// <summary>
    /// Ensures the singleton instance is created when the component awakens.
    /// This prevents race conditions during initialization.
    /// </summary>
    protected virtual void Awake()
    {
        T t = SceneSingleton<T>.Instance; // Force instance creation
    }

    /// <summary>
    /// Gets the singleton instance. Creates it if it doesn't exist.
    /// Thread-safe and handles scene-specific singleton behavior.
    /// </summary>
    public static T Instance
    {
        get
        {
            // Prevent creating instances during application shutdown
            if (applicationIsQuitting)
            {
                Debug.Log("[SceneSingleton] Instance '" + typeof(T) +
                "' already destroyed on application quit." +
                " Won't create again.");
                return _instance;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // First, try to find existing instance in the scene
                    _instance = FindFirstObjectByType<T>();

                    // Check for multiple instances (should never happen)
                    if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
                    {
                        Debug.LogError("[SceneSingleton] Something went really wrong " +
                        " - there should never be more than 1 singleton!" +
                        " Reopenning the scene might fix it.");
                        return _instance;
                    }

                    // If no instance exists, create one
                    if (_instance == null)
                    {
                        // Create empty GameObject and add component
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();

                        // Set meaningful name for debugging
                        _instance.gameObject.name = "~" + typeof(T).ToString();

                        Debug.Log("[SceneSingleton] An instance of " + typeof(T) +
                                  " is needed in the scene, so '" + _instance.gameObject.name +
                                  "' was created.");
                    }
                    else
                    {
                        // Instance already exists in scene
                        Debug.Log("[SceneSingleton] Using instance already created: " + _instance.gameObject.name);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>Flag to track if application is shutting down</summary>
    private static bool applicationIsQuitting = false;

    /// <summary>
    /// Mark as quitting when the application is shutting down.
    /// </summary>
    protected virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}
