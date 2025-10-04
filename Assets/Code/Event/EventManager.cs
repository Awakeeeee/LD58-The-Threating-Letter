using System.Collections.Generic;

namespace Utils
{
    public class EventManager
    {
        private static Dictionary<GameEvent, System.Action<object>> events = new Dictionary<GameEvent, System.Action<object>>();

        public static void StartListening(GameEvent eventName, System.Action<object> listener)
        {
            if (events.TryGetValue(eventName, out System.Action<object> handlers)) //note: out handler is a copy
            {
                handlers -= listener;
                handlers += listener;
                events[eventName] = handlers;
            }
            else
            {
                events.Add(eventName, listener);
            }
        }

        public static void StopListening(GameEvent eventName, System.Action<object> listener)
        {
            if (events.ContainsKey(eventName))
            {
                events[eventName] -= listener;
                if (events[eventName] == null)
                {
                    events.Remove(eventName);
                }
            }
        }

        public static void TriggerEvent(GameEvent eventName, object eventParam = null)
        {
            if (events.TryGetValue(eventName, out System.Action<object> handlers))
            {
                if (handlers != null)
                {
                    handlers.Invoke(eventParam);
                }
            }
        }
    }
}