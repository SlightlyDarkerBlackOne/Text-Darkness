using System.Collections.Generic;

[System.Serializable]
public class RoomData
{
    public string title;
    public string[] description;
    public Dictionary<string, Choice> choices;
    public bool hasHealthEvent;
    public string healthEventText;
} 