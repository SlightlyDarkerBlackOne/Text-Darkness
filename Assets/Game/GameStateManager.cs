using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

[Serializable]
public class Choice
{
    public string id;
    public string[] text_variations;
    public string text;
    public string result;
    public string requires_item;
    public string[] requires_choices;
    public string next_room;
    public string[] unlocks;
    public string adds_to_inventory;
    public string sets_global_flag;
    public string ending;
    public JToken puzzle;
    public string reveals_title;
}

[Serializable]
public class RoomData
{
    public string title;
    public string description;
    public Dictionary<string, Choice> choices;
}

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private TextAsset m_initialRoomJson;
    [SerializeField] private TextMeshProUGUI m_titleText;
    [SerializeField] private TextMeshProUGUI m_mainText;
    [SerializeField] private TMP_InputField m_promptInputField;
    [SerializeField] private NotificationManager m_notificationManager;
    [SerializeField] private bool m_allowEnter = true;
    [SerializeField] private float m_typingSpeed = 0.05f;
    [SerializeField] private TextFader m_titleFader;

    private Dictionary<string, RoomData> m_gameRooms;
    private RoomData m_currentRoom;
    private Coroutine m_typingCoroutine;
    private Dictionary<string, Choice> m_currentChoices;
    private Queue<string> m_textQueue = new Queue<string>();
    private bool m_isTyping = false;
    private HashSet<string> m_completedChoices = new HashSet<string>();
    private string m_currentTypingText = "";
    private HashSet<string> m_inventory = new HashSet<string>();
    private Dictionary<string, bool> m_puzzleStates = new Dictionary<string, bool>();
    private Choice m_currentPuzzle = null;
    private bool m_isFirstRoom = true;
    
    public RoomData CurrentRoom => m_currentRoom;
    public bool LastChoiceWasValid { get; private set; }
    public bool IsTyping => m_isTyping;
    public bool IsInRoom(string roomKey) => m_currentRoom == m_gameRooms[roomKey];
    public bool HasItem(string itemName) => m_inventory.Contains(itemName);
    
    private void Start()
    {
        m_mainText.text = "";
        m_titleText.text = "";
        m_mainText.GetComponent<Button>().onClick.AddListener(SkipTypewriter);
        LoadRoom(m_initialRoomJson);
    }

    private void Update()
    {
        // Keep input field focused
        if (!m_promptInputField.isFocused)
        {
            m_promptInputField.ActivateInputField();
            m_promptInputField.Select();
        }

        if (m_allowEnter && (m_promptInputField.text.Length > 0) 
            && (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
        {
            ProcessChoice(m_promptInputField.text);
            m_promptInputField.text = "";
            // Refocus after processing
            m_promptInputField.ActivateInputField();
            m_promptInputField.Select();
        }
    }

    public void LoadRoom(TextAsset _roomJson)
    {
        try
        {
            m_gameRooms = GameDataDeserializer.DeserializeGameData(_roomJson.text);
            
            if (m_gameRooms != null && m_gameRooms.Count > 0)
            {
                var roomEnumerator = m_gameRooms.GetEnumerator();
                roomEnumerator.MoveNext();
                m_currentRoom = roomEnumerator.Current.Value;
            DisplayCurrentRoom();
            }
            else
            {
                Debug.LogError("No rooms found in the JSON data");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading room data: {e.Message}\n{e.StackTrace}");
        }
    }

    private void DisplayCurrentRoom()
    {
        if (m_currentRoom == null) return;
        
        // Display title if available
        if (!string.IsNullOrEmpty(m_currentRoom.title))
        {
            m_titleFader.FadeIn(m_currentRoom.title, m_titleText);
        }
        
        // Queue the room description with newlines if not first room
        if (!m_isFirstRoom)
        {
            m_textQueue.Enqueue("\n\n\n" + m_currentRoom.description);
        }
        else
        {
            m_textQueue.Enqueue(m_currentRoom.description);
            m_isFirstRoom = false;
        }
        
        // Set current choices
        m_currentChoices = m_currentRoom.choices;

        // Start processing the queue if not already typing
        if (!m_isTyping)
        {
            StartNextText();
        }
    }

    private void StartNextText(Choice _choice = null)
    {
        if (m_textQueue.Count > 0)
        {
            string nextText = m_textQueue.Dequeue();
            m_typingCoroutine = StartCoroutine(TypeText(nextText, _choice));
        }
    }

    private void SkipTypewriter()
    {
        if (m_isTyping && !string.IsNullOrEmpty(m_currentTypingText))
        {
            // Stop the current coroutine
            if (m_typingCoroutine != null)
            {
                StopCoroutine(m_typingCoroutine);
                m_typingCoroutine = null;
            }

            // Instantly show the full text
            m_mainText.text += m_currentTypingText;
            m_currentTypingText = "";
            
            // Reset states
            m_isTyping = false;
            m_allowEnter = true;

            // Continue with next text if any
            StartNextText();

            // Refocus input field
            m_promptInputField.ActivateInputField();
            m_promptInputField.Select();
        }
    }

    private IEnumerator TypeText(string _text, Choice _choice = null)
    {
        m_isTyping = true;
        m_allowEnter = false;
        m_currentTypingText = _text;

        if (m_mainText.text.Length > 500)
        {
            //m_mainText.text = "";
        }
        
        int currentIndex = 0;
        string typedSoFar = "";
        
        foreach (char c in _text)
        {
            m_mainText.text += c;
            currentIndex++;
            typedSoFar += c;
            m_currentTypingText = _text.Substring(currentIndex);

            CheckForTitleReveal(_choice, typedSoFar);

            yield return new WaitForSeconds(m_typingSpeed);
        }

        m_currentTypingText = "";
        m_typingCoroutine = null;
        m_allowEnter = true;
        m_isTyping = false;

        // Process next text in queue if any
        StartNextText();
    }

    private void CheckForTitleReveal(Choice _choice, string _typedSoFar)
    {
        if (_choice != null &&
            !string.IsNullOrEmpty(_choice.reveals_title) &&
            _typedSoFar.ToLower().EndsWith(_choice.reveals_title.ToLower()))
        {
            m_currentRoom.title = _choice.reveals_title;
            m_titleFader.FadeIn(_choice.reveals_title, m_titleText);
        }
    }

    private bool IsPuzzleComplete(Choice _choice, string _input)
    {
        if (_choice.puzzle == null) return true;
        
        var puzzle = _choice.puzzle;
        if (puzzle["type"].ToString() == "number_code")
        {
            string correctCode = puzzle["correct_code"].ToString();
            return _input == correctCode;
        }
        else if (puzzle["type"].ToString() == "symbol_sequence")
        {
            //string correctSequence = puzzle["correct_sequence"].ToString();
            //return _input == correctSequence;
            return true;
        }
        
        return false;
    }

    public void ProcessChoice(string _inputText)
    {
        LastChoiceWasValid = false;
        
        // Handle developer commands
        if (_inputText.StartsWith("/"))
        {
            ProcessDevCommand(_inputText);
            m_promptInputField.text = "";
            return;
        }

        // If we're in a puzzle state, check the puzzle input
        if (m_currentPuzzle != null)
        {
            if (IsPuzzleComplete(m_currentPuzzle, _inputText))
            {
                m_puzzleStates[m_currentPuzzle.id] = true;
                
                // Add items to inventory if puzzle is complete
                if (!string.IsNullOrEmpty(m_currentPuzzle.adds_to_inventory))
                {
                    m_inventory.Add(m_currentPuzzle.adds_to_inventory);
                    m_notificationManager.ShowNotification(m_currentPuzzle.adds_to_inventory);
                }

                // Queue the result text with newlines
                m_textQueue.Enqueue("\n\n" + m_currentPuzzle.result);

                // Handle room transition
                if (!string.IsNullOrEmpty(m_currentPuzzle.next_room))
                {
                    if (m_gameRooms != null && m_gameRooms.ContainsKey(m_currentPuzzle.next_room))
                    {
                        m_currentRoom = m_gameRooms[m_currentPuzzle.next_room];
                        DisplayCurrentRoom(); // This will queue the new room description with newlines
                    }
                }

                m_currentPuzzle = null;
                LastChoiceWasValid = true;
            }
            else
            {
                Debug.LogError("That's not the correct solution.");
            }
            return;
        }

        if (m_currentChoices == null)
        {
            Debug.LogError("Current choices are null. Ensure that the current room has choices defined.");
            return;
        }

        // Find matching choice based on text variations
        Choice matchedChoice = null;
        string matchedKey = null;

        foreach (var choice in m_currentChoices)
        {
            if (choice.Value.text_variations.Any(variation => 
                _inputText.ToLower().Contains(variation.ToLower())))
            {
                matchedChoice = choice.Value;
                matchedKey = choice.Key;
                break;
            }
        }

        if (matchedChoice == null)
        {
            Debug.LogError($"Invalid choice: {_inputText}. No matching action found.");
            return;
        }

        // Check if all required choices are completed
        if (matchedChoice.requires_choices != null && 
            matchedChoice.requires_choices.Any(req => !m_completedChoices.Contains(req)))
        {
            Debug.LogError("You can't do that yet. Something else must be done first.");
            return;
        }
        
        // Check if the choice has requirements
        if (!string.IsNullOrEmpty(matchedChoice.requires_item) && !m_inventory.Contains(matchedChoice.requires_item))
        {
            Debug.LogError($"You need {matchedChoice.requires_item} to do that.");
            return;
        }

        // If this choice has a puzzle, set it as current puzzle
        if (matchedChoice.puzzle != null)
        {
            m_currentPuzzle = matchedChoice;
            m_textQueue.Enqueue("\n\n" + matchedChoice.result);
            LastChoiceWasValid = true;
            return;
        }

        // Add this choice to completed choices
        m_completedChoices.Add(matchedChoice.id);

        // Add items to inventory only if no puzzle
        if (!string.IsNullOrEmpty(matchedChoice.adds_to_inventory))
        {
            m_inventory.Add(matchedChoice.adds_to_inventory);
            m_notificationManager.ShowNotification(matchedChoice.adds_to_inventory);
        }

        // Queue the result text with newlines
        m_textQueue.Enqueue("\n\n" + matchedChoice.result);

        // Handle room transition
        if (!string.IsNullOrEmpty(matchedChoice.next_room))
        {
            if (m_gameRooms != null && m_gameRooms.ContainsKey(matchedChoice.next_room))
            {
                m_currentRoom = m_gameRooms[matchedChoice.next_room];
                DisplayCurrentRoom(); // This will queue the new room description with newlines
            }
            else
            {
                Debug.LogWarning($"Room not found: {matchedChoice.next_room}");
            }
        }
        else if (!m_isTyping)
        {
            StartNextText(matchedChoice);
        }

        LastChoiceWasValid = true;
    }

    private void ProcessDevCommand(string _command)
    {
        switch (_command.ToLower())
        {
            case "/choices":
                string choicesText = "\n\nAvailable Choices:";
                foreach (var choice in m_currentChoices)
                {
                    // Add requirements info if any
                    if (choice.Value.requires_choices != null && choice.Value.requires_choices.Length > 0)
                        continue;
                        
                    choicesText += $"\n- {choice.Value.text}";
                    
                    if (!string.IsNullOrEmpty(choice.Value.requires_item))
                        choicesText += $" (Requires {choice.Value.requires_item})";
                }
                m_textQueue.Enqueue(choicesText);
                break;

            case "/finished":
                string finishedText = "\n\nCompleted Actions:";
                if (m_completedChoices.Count == 0)
                {
                    finishedText += "\nNo actions completed yet.";
                }
                else
                {
                    foreach (string choiceId in m_completedChoices)
                    {
                        // Find the choice in any room to get its display text
                        foreach (var room in m_gameRooms.Values)
                        {
                            var choice = room.choices.Values.FirstOrDefault(c => c.id == choiceId);
                            if (choice != null)
                            {
                                finishedText += $"\n- {choice.text}";
                                break;
                            }
                        }
                    }
                }
                m_textQueue.Enqueue(finishedText);
                break;

            default:
                m_textQueue.Enqueue($"\n\nUnknown command: {_command}");
                break;
        }

        if (!m_isTyping)
        {
            StartNextText();
        }
    }
}