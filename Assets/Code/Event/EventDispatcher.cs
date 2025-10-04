using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class EventPayload
    {
    }

    public class EventDispatcher<T>
        where T : System.Enum
    {
        public delegate void EventHandler(T key, EventPayload data);

        private Dictionary<T, EventHandler> events;

        public EventDispatcher()
        {
            events = new Dictionary<T, EventHandler>();
        }

        public void AddListener(T key, EventHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            if (!events.TryGetValue(key, out EventHandler targetHandler))
            {
                events.Add(key, handler);
                return;
            }
            else
            {
                events[key] -= handler;
                events[key] += handler;
            }
        }

        public void RemoveListener(T key, EventHandler handler)
        {
            if (handler == null)
            {
                return;
            }

            if (events.TryGetValue(key, out EventHandler outHandler))
            {
                events[key] -= handler;
            }
        }

        public void Dispatch(T key, EventPayload payload)
        {
            if (!events.TryGetValue(key, out EventHandler outHandler))
            {
                Debug.LogWarning(string.Format("EventDispatcher: {0} has not register handlers", key.ToString()));
                return;
            }

            events[key]?.Invoke(key, payload);
        }
    }
}
