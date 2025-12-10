using UnityEngine;

/// <summary>
/// Interface for objects that can be grabbed by the grappling hand.
/// Implement this interface on any component you want to be grabbable.
/// </summary>
public interface IGrabbable
{
    /// <summary>
    /// Called to check if the object can currently be grabbed
    /// </summary>
    /// <returns>True if the object can be grabbed</returns>
    bool CanBeGrabbed();
    
    /// <summary>
    /// Called when the object is grabbed by the grappling hand
    /// </summary>
    /// <param name="player">The player transform that grabbed this object</param>
    void OnGrabbed(Transform player);
    
    /// <summary>
    /// Called when the object is released (when hand returns to player)
    /// </summary>
    void OnReleased();
}

