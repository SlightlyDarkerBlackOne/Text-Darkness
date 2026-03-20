using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System;

public class RoomManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_titleText;
    [SerializeField] private TextMeshProUGUI m_mainText;
    [SerializeField] private TextFader m_titleFader;
    [SerializeField] private Button m_skipButton;
    [SerializeField] private TMP_InputField m_promptInputField;
    [SerializeField] private ChoiceManager m_choiceManager;
    [SerializeField] private float m_typingSpeed = 0.05f;
    [SerializeField] private float m_delayBetweenRows = 0.5f;
    [SerializeField] private bool m_allowTextInput = true;
    private Dictionary<string, Choice> m_currentChoices;

    private RoomData m_currentRoom;
    private Coroutine m_typingCoroutine;
    private string m_currentTypingText = "";
    private bool m_isTyping = false;
    private bool m_isFirstRoom = true;
    private Queue<string> m_textQueue = new Queue<string>();

    public Dictionary<string, RoomData> GameRooms {get; private set;}
    public RoomData CurrentRoom => m_currentRoom;
    public bool IsTyping => m_isTyping;
    public bool IsInRoom(string roomKey) => m_currentRoom == GameRooms[roomKey];


    public void Initialize(TextAsset _roomJson)
    {
        m_mainText.text = "";
        m_titleText.text = "";
        m_skipButton.onClick.AddListener(SkipTypewriter);
        LoadRoom(_roomJson);
    }

    private void Update()
    {
        // Keep input field focused
        if (!m_promptInputField.isFocused)
        {
            m_promptInputField.ActivateInputField();
            m_promptInputField.Select();
        }

        if(Input.GetKeyDown(KeyCode.Return) && IsTyping)
        {
            SkipTypewriter();
        }

        if (m_allowTextInput && m_promptInputField.text.Length > 0 && 
            (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
        {
            m_choiceManager.ProcessChoice(m_promptInputField.text);
            m_promptInputField.text = "";
            m_promptInputField.ActivateInputField();
            m_promptInputField.Select();
        }
    } 

    private void InitializeRooms(Dictionary<string, RoomData> _gameRooms)
    {
        GameRooms = _gameRooms;
        if (GameRooms != null && GameRooms.Count > 0)
        {
            var roomEnumerator = GameRooms.GetEnumerator();
            roomEnumerator.MoveNext();
            m_currentRoom = roomEnumerator.Current.Value;
            DisplayCurrentRoom();
        }
    }

    public void LoadRoom(TextAsset _roomJson)
    {
        try
        {
            GameRooms = GameDataDeserializer.DeserializeGameData(_roomJson.text);
            
            if (GameRooms != null && GameRooms.Count > 0)
            {
                InitializeRooms(GameRooms);
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

    public void DisplayCurrentRoom()
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
            m_textQueue.Enqueue("\n\n\n");
            foreach (var result in m_currentRoom.description)
            {
                m_textQueue.Enqueue("\n" + result);
            }
        }
        else
        {
            foreach (var result in m_currentRoom.description)
            {
                m_textQueue.Enqueue("\n" + result);
            }
            m_isFirstRoom = false;
        }

        // Set current choices
        m_currentChoices = m_currentRoom.choices;
        m_choiceManager.Initialize(m_currentChoices);

        if (!m_isTyping)
        {
            StartNextText();
        }
    }

    public void StartNextText(Choice _choice = null)
    {
        if (m_textQueue.Count > 0)
        {
            string nextText = m_textQueue.Dequeue();
            m_typingCoroutine = StartCoroutine(TypeText(nextText, _choice));
        }
    }

    private IEnumerator TypeText(string _text, Choice _choice = null)
    {
        m_isTyping = true;
        m_allowTextInput = false;
        m_currentTypingText = _text;
        
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
        m_allowTextInput = true;
        m_isTyping = false;

        yield return new WaitForSeconds(m_delayBetweenRows);

        // Check if this is the line with the health event
        if (m_currentRoom.hasHealthEvent && _text.Contains(m_currentRoom.healthEventText))
        {
            GameStateManager.TriggerEventHealth();
        }

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
            m_allowTextInput = true;

            // Continue with next text if any
            StartNextText();

            // Refocus input field
            m_promptInputField.ActivateInputField();
            m_promptInputField.Select();
        }
    }

    public void TransitionToRoom(string _roomKey)
    {
        if (GameRooms != null && GameRooms.ContainsKey(_roomKey))
        {
            m_currentRoom = GameRooms[_roomKey];
            DisplayCurrentRoom();
        }
        else
        {
            Debug.LogWarning($"Room not found: {_roomKey}");
        }
    }

    public void QueueText(string _text)
    {
        m_textQueue.Enqueue(_text);
        if (!m_isTyping)
        {
            StartNextText();
        }
    }
} 