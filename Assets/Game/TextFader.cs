using UnityEngine;
using TMPro;
using System.Collections;

public class TextFader : MonoBehaviour
{
    [SerializeField] private float m_fadeDuration = 1.5f;
    private Coroutine m_fadeCoroutine;

    public void FadeIn(string _text, TextMeshProUGUI _textComponent, float _delay = 0f)
    {
        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
        }
        m_fadeCoroutine = StartCoroutine(FadeInRoutine(_text, _textComponent, _delay));
    }

    public void Clear(TextMeshProUGUI _textComponent)
    {
        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
        }
        _textComponent.text = "";
        _textComponent.alpha = 0f;
    }

    private IEnumerator FadeInRoutine(string _text, TextMeshProUGUI _textComponent, float _delay)
    {
        yield return new WaitForSeconds(_delay);

        _textComponent.text = _text;
        _textComponent.alpha = 0f;

        float elapsedTime = 0f;

        while (elapsedTime < m_fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / m_fadeDuration;
            
            // Smooth step for more pleasing fade
            float alpha = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);
            _textComponent.alpha = alpha;
            
            yield return null;
        }

        _textComponent.alpha = 1f;
        m_fadeCoroutine = null;
    }
} 