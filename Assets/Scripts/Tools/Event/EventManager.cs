using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace GameTool
{
    
    // EventManager : Listen and Trigger events
    // - Same event name with different argument treat as different event
    /// <summary>
    /// Centralized pub/sub event hub.
    /// - Events are keyed by name plus payload type(s). The same name with different payload types are treated as distinct events.
    /// - Use StartListening/StopListening to subscribe/unsubscribe.
    /// - Use TriggerEvent to broadcast to current listeners.
    /// - Supports: no-arg, single generic arg T, and two generic args T1,T2.
    /// </summary>
    public static class EventManager
    {
        [System.Serializable]
        public struct EventKey
        {
            public string EventName;
            public Type ParamType1;
            public Type ParamType2;

            public EventKey(string eventName, Type type1 = null, Type type2 = null)
            {
                EventName = eventName;
                ParamType1 = type1;
                ParamType2 = type2;
            }

            public override string ToString()
            {
                return EventName +
                        ((ParamType1 != null) ? " - [" + ParamType1.Name + "]" : "") +
                        ((ParamType2 != null) ? " - [" + ParamType2.Name + "]" : "");
            }
        }

        private static Dictionary<EventKey, object> RegisteredEvents = new Dictionary<EventKey, object>();

        public static ReadOnlyDictionary<EventKey, object> GetRegisteredEvents()
        {
            return new ReadOnlyDictionary<EventKey, object>(RegisteredEvents);
        }

        /// <summary>
        /// Subscribe to a no-argument event.
        /// </summary>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Callback invoked when the event is triggered.</param>
        public static void StartListening(string eventName, UnityAction listener)
        {
            EventKey thisInfo = new EventKey(eventName);

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent)thisEvent).AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                ((UnityEvent)thisEvent).AddListener(listener);
                RegisteredEvents.Add(thisInfo, thisEvent);
            }
        }

        /// <summary>
        /// Unsubscribe from a no-argument event.
        /// </summary>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Previously subscribed callback.</param>
        public static void StopListening(string eventName, UnityAction listener)
        {
            EventKey thisInfo = new EventKey(eventName);

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent)thisEvent).RemoveListener(listener);
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }

        /// <summary>
        /// Trigger a no-argument event.
        /// </summary>
        /// <param name="eventName">Event name key.</param>
        public static void TriggerEvent(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName);

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent)thisEvent).Invoke();
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }

        /// <summary>
        /// Subscribe to a single-argument event.
        /// </summary>
        /// <typeparam name="T">Payload type.</typeparam>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Callback invoked with payload when triggered.</param>
        public static void StartListening<T>(string eventName, UnityAction<T> listener)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T));

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T>)thisEvent).AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent<T>();
                ((UnityEvent<T>)thisEvent).AddListener(listener);
                RegisteredEvents.Add(thisInfo, thisEvent);
            }
        }

        /// <summary>
        /// Unsubscribe from a single-argument event.
        /// </summary>
        /// <typeparam name="T">Payload type.</typeparam>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Previously subscribed callback.</param>
        public static void StopListening<T>(string eventName, UnityAction<T> listener)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T));

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T>)thisEvent).RemoveListener(listener);
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }

        /// <summary>
        /// Trigger a single-argument event.
        /// </summary>
        /// <typeparam name="T">Payload type.</typeparam>
        /// <param name="eventName">Event name key.</param>
        /// <param name="message">Payload to pass to listeners.</param>
        public static void TriggerEvent<T>(string eventName, T message)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T));

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T>)thisEvent).Invoke(message);
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }

        /// <summary>
        /// Subscribe to a two-argument event.
        /// </summary>
        /// <typeparam name="T1">First payload type.</typeparam>
        /// <typeparam name="T2">Second payload type.</typeparam>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Callback invoked with both payloads.</param>
        public static void StartListening<T1, T2>(string eventName, UnityAction<T1, T2> listener)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T1), typeof(T2));

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T1, T2>)thisEvent).AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent<T1, T2>();
                ((UnityEvent<T1, T2>)thisEvent).AddListener(listener);
                RegisteredEvents.Add(thisInfo, thisEvent);
            }
        }

        /// <summary>
        /// Unsubscribe from a two-argument event.
        /// </summary>
        /// <typeparam name="T1">First payload type.</typeparam>
        /// <typeparam name="T2">Second payload type.</typeparam>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Previously subscribed callback.</param>
        public static void StopListening<T1, T2>(string eventName, UnityAction<T1, T2> listener)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T1), typeof(T2));

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T1, T2>)thisEvent).RemoveListener(listener);
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }

        /// <summary>
        /// Trigger a two-argument event.
        /// </summary>
        /// <typeparam name="T1">First payload type.</typeparam>
        /// <typeparam name="T2">Second payload type.</typeparam>
        /// <param name="eventName">Event name key.</param>
        /// <param name="message1">First payload.</param>
        /// <param name="message2">Second payload.</param>
        public static void TriggerEvent<T1, T2>(string eventName, T1 message1, T2 message2)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T1), typeof(T2));

            if (RegisteredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T1, T2>)thisEvent).Invoke(message1, message2);
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }

        /// <summary>
        /// Clear all registered events. Useful for scene transitions or cleanup.
        /// </summary>
        public static void ClearAllEvents()
        {
            RegisteredEvents.Clear();
            Debug.Log("All events cleared from EventManager");
        }

        /// <summary>
        /// Remove a specific event from the registry.
        /// </summary>
        /// <param name="eventName">Event name to remove.</param>
        public static void RemoveEvent(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName);
            if (RegisteredEvents.Remove(thisInfo))
            {
                Debug.Log($"Event removed: {thisInfo.ToString()}");
            }
            else
            {
                Debug.LogWarning($"Event not found for removal: {thisInfo.ToString()}");
            }
        }

        /// <summary>
        /// Remove a specific single-argument event from the registry.
        /// </summary>
        /// <typeparam name="T">Payload type.</typeparam>
        /// <param name="eventName">Event name to remove.</param>
        public static void RemoveEvent<T>(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T));
            if (RegisteredEvents.Remove(thisInfo))
            {
                Debug.Log($"Event removed: {thisInfo.ToString()}");
            }
            else
            {
                Debug.LogWarning($"Event not found for removal: {thisInfo.ToString()}");
            }
        }

        /// <summary>
        /// Remove a specific two-argument event from the registry.
        /// </summary>
        /// <typeparam name="T1">First payload type.</typeparam>
        /// <typeparam name="T2">Second payload type.</typeparam>
        /// <param name="eventName">Event name to remove.</param>
        public static void RemoveEvent<T1, T2>(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T1), typeof(T2));
            if (RegisteredEvents.Remove(thisInfo))
            {
                Debug.Log($"Event removed: {thisInfo.ToString()}");
            }
            else
            {
                Debug.LogWarning($"Event not found for removal: {thisInfo.ToString()}");
            }
        }

        /// <summary>
        /// Get the number of registered events.
        /// </summary>
        /// <returns>Count of registered events.</returns>
        public static int GetEventCount()
        {
            return RegisteredEvents.Count;
        }

        /// <summary>
        /// Check if an event is registered.
        /// </summary>
        /// <param name="eventName">Event name to check.</param>
        /// <returns>True if event exists, false otherwise.</returns>
        public static bool HasEvent(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName);
            return RegisteredEvents.ContainsKey(thisInfo);
        }

        /// <summary>
        /// Check if a single-argument event is registered.
        /// </summary>
        /// <typeparam name="T">Payload type.</typeparam>
        /// <param name="eventName">Event name to check.</param>
        /// <returns>True if event exists, false otherwise.</returns>
        public static bool HasEvent<T>(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T));
            return RegisteredEvents.ContainsKey(thisInfo);
        }

        /// <summary>
        /// Check if a two-argument event is registered.
        /// </summary>
        /// <typeparam name="T1">First payload type.</typeparam>
        /// <typeparam name="T2">Second payload type.</typeparam>
        /// <param name="eventName">Event name to check.</param>
        /// <returns>True if event exists, false otherwise.</returns>
        public static bool HasEvent<T1, T2>(string eventName)
        {
            EventKey thisInfo = new EventKey(eventName, typeof(T1), typeof(T2));
            return RegisteredEvents.ContainsKey(thisInfo);
        }
    }
}
