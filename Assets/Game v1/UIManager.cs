using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_descriptionText;
    [SerializeField] private GameObject m_choiceButtonPrefab;
    [SerializeField] private Transform m_choicesContainer;
    [SerializeField] private float m_typingSpeed = 0.05f;

    private List<GameObject> m_activeChoiceButtons = new List<GameObject>();

    public void ClearText()
    {
        m_descriptionText.text = string.Empty;
    }

    public IEnumerator TypeText(string _text)
    {
        m_descriptionText.text = string.Empty;
        foreach (char c in _text)
        {
            m_descriptionText.text += c;
            yield return new WaitForSeconds(m_typingSpeed);
        }
    }

    public void DisplayChoices(List<GameData.Choice> _choices, System.Action<GameData.Choice> _onChoiceSelected)
    {
        ClearChoices();

        foreach (var choice in _choices)
        {
            var buttonObj = Instantiate(m_choiceButtonPrefab, m_choicesContainer);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            text.text = choice.Text;
            button.onClick.AddListener(() => _onChoiceSelected(choice));

            m_activeChoiceButtons.Add(buttonObj);
        }
    }

    private void ClearChoices()
    {
        foreach (var button in m_activeChoiceButtons)
        {
            Destroy(button);
        }
        m_activeChoiceButtons.Clear();
    }
} 