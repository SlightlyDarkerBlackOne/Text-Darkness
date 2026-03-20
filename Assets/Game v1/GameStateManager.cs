using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private TextAsset m_initialRoomJson;
    [SerializeField] private RoomManager m_roomManager;

    private void Start()
    {
        m_roomManager.Initialize(m_initialRoomJson);
    }

    public static void TriggerEventHealth()
    {
        // TODO: Implement health mechanic introduction
        Debug.Log("Health mechanic introduced");
    }
}