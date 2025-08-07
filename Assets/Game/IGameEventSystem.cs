using UnityEngine;

public interface IGameEventSystem
{
    void TriggerEvent(GameEventType _eventType, string _parameter = null);
    void RegisterListener(GameEventType _eventType, System.Action<string> _callback);
    void UnregisterListener(GameEventType _eventType, System.Action<string> _callback);
}

public enum GameEventType
{
    IntroduceMechanic,
    PlaySound,
    ChangeMusic,
    FadeUI,
    HealthEvent
} 