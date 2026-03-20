using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/Game Data")]
public class GameData : ScriptableObject
{
    [System.Serializable]
    public class Room
    {
        public string Id;
        public string Description;
        public List<Choice> Choices = new List<Choice>();
    }

    [System.Serializable]
    public class Choice
    {
        public string Text;
        public string NextRoomId;
    }

    public List<Room> Rooms = new List<Room>();
    public string StartingRoomId;
} 