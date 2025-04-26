using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ChoiceManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_promptInputField;
    [SerializeField] private NotificationManager m_notificationManager;
    [SerializeField] private RoomManager m_roomManager;

    private Dictionary<string, Choice> m_currentChoices;
    private HashSet<string> m_completedChoices = new HashSet<string>();
    private HashSet<string> m_inventory = new HashSet<string>();
    private Dictionary<string, bool> m_puzzleStates = new Dictionary<string, bool>();
    private Choice m_currentPuzzle = null;

    public bool LastChoiceWasValid { get; private set; }
    public bool HasItem(string _itemName) => m_inventory.Contains(_itemName);

    public void Initialize(Dictionary<string, Choice> _choices)
    {
        m_currentChoices = _choices;
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
                HandlePuzzleCompleted();
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

        // Queue the initial result text immediately
        m_roomManager.QueueText("\n\n" + string.Join("\n", matchedChoice.result));

        // If this choice has a puzzle, set it as current puzzle
        if (matchedChoice.puzzle != null)
        {
            m_currentPuzzle = matchedChoice;
            LastChoiceWasValid = true;
            m_roomManager.StartNextText(matchedChoice);
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

        // Handle room transition
        if (!string.IsNullOrEmpty(matchedChoice.next_room))
        {
            m_roomManager.TransitionToRoom(matchedChoice.next_room);
        }
        else if (!m_roomManager.IsTyping)
        {
            m_roomManager.StartNextText(matchedChoice);
        }

        LastChoiceWasValid = true;
    }

    private bool IsPuzzleComplete(Choice _choice, string _input)
    {
        if (_choice.puzzle == null) return true;
        
        var puzzle = _choice.puzzle;
        if (puzzle.type == "number_code")
        {
            string correctCode = puzzle.correct_code;
            return _input == correctCode;
        }
        else if (puzzle.type == "symbol_sequence")
        {
            return true;
        }
        
        return false;
    }

    private void HandlePuzzleCompleted()
    {
        m_puzzleStates[m_currentPuzzle.id] = true;

        if (m_currentPuzzle.puzzle.adds_to_inventory != null)
        {
            string item = m_currentPuzzle.puzzle.adds_to_inventory;
            m_inventory.Add(item);
            m_notificationManager.ShowNotification(item);
        }

        if (m_currentPuzzle.puzzle.result != null)
        {
            m_roomManager.QueueText("\n\n" + m_currentPuzzle.puzzle.result);
            m_roomManager.StartNextText(m_currentPuzzle);
        }

        // Handle room transition
        if (!string.IsNullOrEmpty(m_currentPuzzle.next_room))
        {
            m_roomManager.TransitionToRoom(m_currentPuzzle.next_room);
        }

        m_currentPuzzle = null;
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
                    if (choice.Value.requires_choices != null && choice.Value.requires_choices.Length > 0)
                        continue;
                        
                    choicesText += $"\n- {choice.Value.text}";
                    
                    if (!string.IsNullOrEmpty(choice.Value.requires_item))
                        choicesText += $" (Requires {choice.Value.requires_item})";
                }
                m_roomManager.QueueText(choicesText);
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
                        foreach (var room in m_roomManager.GameRooms.Values)
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
                m_roomManager.QueueText(finishedText);
                break;

            default:
                m_roomManager.QueueText($"\n\nUnknown command: {_command}");
                break;
        }

        if (!m_roomManager.IsTyping)
        {
            m_roomManager.StartNextText();
        }
    }
} 