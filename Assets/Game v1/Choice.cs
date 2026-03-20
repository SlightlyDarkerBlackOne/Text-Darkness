[System.Serializable]
public class Choice
{
    public string id;
    public string[] text_variations;
    public string text;
    public string[] result;
    public string requires_item;
    public string[] requires_choices;
    public string next_room;
    public string[] unlocks;
    public string adds_to_inventory;
    public string sets_global_flag;
    public string ending;
    public Puzzle puzzle;
    public string reveals_title;
} 