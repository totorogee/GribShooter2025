using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;

namespace GameTool
{
    
    public static partial class EventName 
{
    public static string InitHexAtMouse         = "InitHexAtMouse";
    public static string ClearTileAtMouse       = "ClearTileAtMouse";
    public static string InitHexByHexInt        = "InitHexByHexInt";
    public static string InitTilesHexRing       = "InitTilesHexRing";
    public static string InitTilesHexPlane      = "InitTilesHexPlane";
    public static string InitTilesCirPlane      = "InitTilesCirPlane";
    public static string ClearAllTiles          = "ClearAllTiles";
    public static string DisplayDataByHexInt    = "DisplayDataByHexInt";
    
}

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

        private static Dictionary<EventKey, object> RegistoredEvents = new Dictionary<EventKey, object>();

        public static ReadOnlyDictionary<EventKey, object> GetRegistoredEvents()
        {
            return new ReadOnlyDictionary<EventKey, object>(RegistoredEvents);
        }

        /// <summary>
        /// Subscribe to a no-argument event.
        /// </summary>
        /// <param name="eventName">Event name key.</param>
        /// <param name="listener">Callback invoked when the event is triggered.</param>
        public static void StartListening(string eventName, UnityAction listener)
        {
            EventKey thisInfo = new EventKey(eventName);

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent)thisEvent).AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                ((UnityEvent)thisEvent).AddListener(listener);
                RegistoredEvents.Add(thisInfo, thisEvent);
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T>)thisEvent).AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent<T>();
                ((UnityEvent<T>)thisEvent).AddListener(listener);
                RegistoredEvents.Add(thisInfo, thisEvent);
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T1, T2>)thisEvent).AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent<T1, T2>();
                ((UnityEvent<T1, T2>)thisEvent).AddListener(listener);
                RegistoredEvents.Add(thisInfo, thisEvent);
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
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

            if (RegistoredEvents.TryGetValue(thisInfo, out object thisEvent))
            {
                ((UnityEvent<T1, T2>)thisEvent).Invoke(message1, message2);
            }
            else
            {
                Debug.LogWarning("Event not registered : " + thisInfo.ToString());
            }
        }
    }
}
