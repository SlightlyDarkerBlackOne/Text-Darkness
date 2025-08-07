using UnityEngine;
using System.Collections.Generic;
using System;

public class GameEventSystem : MonoBehaviour, IGameEventSystem
{
    private Dictionary<GameEventType, List<Action<string>>> m_eventListeners = 
        new Dictionary<GameEventType, List<Action<string>>>();

    public void TriggerEvent(GameEventType _eventType, string _parameter = null)
    {
        if (m_eventListeners.TryGetValue(_eventType, out var listeners))
        {
            foreach (var listener in listeners)
            {
                listener?.Invoke(_parameter);
            }
        }
    }

    public void RegisterListener(GameEventType _eventType, Action<string> _callback)
    {
        if (!m_eventListeners.ContainsKey(_eventType))
        {
            m_eventListeners[_eventType] = new List<Action<string>>();
        }
        m_eventListeners[_eventType].Add(_callback);
    }

    public void UnregisterListener(GameEventType _eventType, Action<string> _callback)
    {
        if (m_eventListeners.TryGetValue(_eventType, out var listeners))
        {
            listeners.Remove(_callback);
        }
    }
} 