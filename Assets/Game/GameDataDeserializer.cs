using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

public class GameDataDeserializer
{
    public static Dictionary<string, RoomData> DeserializeGameData(string jsonString)
    {
        try
        {
            var gameData = new Dictionary<string, RoomData>();
            JObject jsonData = JObject.Parse(jsonString);
            JObject rooms = (JObject)jsonData["rooms"];

            foreach (var roomEntry in rooms)
            {
                string roomKey = roomEntry.Key;
                JObject roomData = (JObject)roomEntry.Value;
                
                var newRoom = new RoomData
                {
                    description = roomData["description"].ToString(),
                    choices = new Dictionary<string, Choice>()
                };

                // Parse room title if available
                if (roomData["title"] != null)
                {
                    newRoom.title = roomData["title"].ToString();
                }

                JObject choices = (JObject)roomData["choices"];
                foreach (var choiceEntry in choices)
                {
                    string choiceKey = choiceEntry.Key;
                    JObject choiceData = (JObject)choiceEntry.Value;

                    var newChoice = new Choice
                    {
                        id = choiceData["id"]?.ToString() ?? choiceKey, // Use key as id if not specified
                        text = choiceData["text"].ToString(),
                        result = choiceData["result"].ToString()
                    };

                    // Parse optional fields
                    if (choiceData["text_variations"] != null)
                        newChoice.text_variations = choiceData["text_variations"].ToObject<string[]>();
                    if (choiceData["requires_item"] != null)
                        newChoice.requires_item = choiceData["requires_item"].ToString();
                    if (choiceData["requires_choices"] != null)
                        newChoice.requires_choices = choiceData["requires_choices"].ToObject<string[]>();
                    if (choiceData["next_room"] != null)
                        newChoice.next_room = choiceData["next_room"].ToString();
                    if (choiceData["unlocks"] != null)
                        newChoice.unlocks = choiceData["unlocks"].ToObject<string[]>();
                    if (choiceData["adds_to_inventory"] != null)
                        newChoice.adds_to_inventory = choiceData["adds_to_inventory"].ToString();
                    if (choiceData["sets_global_flag"] != null)
                        newChoice.sets_global_flag = choiceData["sets_global_flag"].ToString();
                    if (choiceData["ending"] != null)
                        newChoice.ending = choiceData["ending"].ToString();
                    if (choiceData["puzzle"] != null)
                        newChoice.puzzle = choiceData["puzzle"];
                    if (choiceData["reveals_title"] != null)
                        newChoice.reveals_title = choiceData["reveals_title"].ToString();

                    // If no text variations provided, create array with just the main text
                    if (newChoice.text_variations == null)
                    {
                        newChoice.text_variations = new string[] { newChoice.text.ToLower() };
                    }

                    // If this choice reveals a title and the room doesn't have one yet,
                    // we'll set it to null initially so it can be revealed later
                    if (!string.IsNullOrEmpty(newChoice.reveals_title) && newRoom.title == null)
                    {
                        newRoom.title = null;
                    }

                    newRoom.choices[choiceKey] = newChoice;
                }

                gameData[roomKey] = newRoom;
            }

            return gameData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deserializing game data: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }
} 