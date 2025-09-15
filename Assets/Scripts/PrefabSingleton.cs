using UnityEngine;

/// <summary>
/// Generic singleton pattern for MonoBehaviour components that can automatically load from prefabs.
/// Ensures only one instance exists throughout the application lifecycle and persists across scenes.
/// 
/// Usage:
/// 1. Create a class that inherits from PrefabSingleton<YourClass>
/// 2. Optionally create a prefab in Resources/Singleton/ folder with the same name as your class
/// 3. Access the instance using YourClass.Instance
/// 
/// Example:
/// public class GameManager : PrefabSingleton<GameManager>
/// {
///     public void DoSomething() { ... }
/// }
/// 
/// // Usage elsewhere:
/// GameManager.Instance.DoSomething();
/// </summary>
/// <typeparam name="T">The MonoBehaviour type that will be made singleton</typeparam>
public class PrefabSingleton<T> : MonoBehaviour where T : MonoBehaviour{

    /// <summary>The singleton instance</summary>
    private static T _instance;

    /// <summary>Thread safety lock for instance creation</summary>
    private static object _lock = new object();

    /// <summary>Flag to prevent multiple creation attempts</summary>
    private static bool created = false;

    /// <summary>
    /// Ensures the singleton instance is created when the component awakens.
    /// This prevents race conditions during initialization.
    /// </summary>
    //Error code: Never used
#pragma warning disable 219
    protected virtual void Awake()
    {
        T t = PrefabSingleton<T>.Instance; // Force instance creation
    }
#pragma warning restore 219

    /// <summary>
    /// Gets the singleton instance. Creates it if it doesn't exist.
    /// Thread-safe and handles prefab loading from Resources folder.
    /// </summary>
    public static T Instance
    {
        get
        {
            // Prevent creating instances during application shutdown
            if (applicationIsQuitting)
            {
                Debug.Log("[Singleton] Instance '" + typeof(T) +
                "' already destroyed on application quit." +
                " Won't create again.");
                return _instance;
            }

            lock (_lock)
            {
                if (_instance == null && !created)
                {
                    // First, try to find existing instance in the scene
                    _instance = FindFirstObjectByType<T>();

                    // Check for multiple instances (should never happen)
                    if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                        " - there should never be more than 1 singleton!" +
                        " Reopenning the scene might fix it.");
                        return _instance;
                    }

                    // If no instance exists, create one
                    if (_instance == null)
                    {
                        created = true;

                        // Try to load prefab from Resources/Singleton/ folder
                        GameObject go = Resources.Load<GameObject>("Singleton/" + typeof(T).Name);

                        if (go != null)
                        {
                            // Instantiate from prefab if it exists
                            _instance = Instantiate<GameObject>(go).GetComponent<T>();
                        }else{
                            // Create empty GameObject and add component if no prefab exists
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                        }

                        // Set meaningful name for debugging and UnitySendMessage callbacks
                        _instance.gameObject.name = "~" + typeof(T).ToString();

                        Debug.Log("[Singleton] An instance of " + typeof(T) +
                                  " is needed in the scene, so '" + _instance.gameObject.name +
                                  "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        // Instance already exists in scene
                        // Uncomment for debugging:
                        // _instance.gameObject.name = "~" + typeof(T).ToString();
                        // Debug.Log("[Singleton] Using instance already created: " + _instance.gameObject.name);
                    }

                    // Make the instance persist across scene loads
                    DontDestroyOnLoad(_instance.transform.root.gameObject);
                }

                return _instance;
            }
        }
    }

    /// <summary>Flag to track if application is shutting down</summary>
    private static bool applicationIsQuitting = false;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public virtual void OnDestroy()
    {
        applicationIsQuitting = true;
    }
}
