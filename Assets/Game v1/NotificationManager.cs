using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_notificationText;
    [SerializeField] private RectTransform m_notificationRect;
    [SerializeField] private float m_notificationShowX = 0f;
    [SerializeField] private float m_notificationHideX = 600f;
    [SerializeField] private float m_notificationDuration = 2.5f;
    [SerializeField] private float m_slideDuration = 0.5f;

    private Coroutine m_notificationCoroutine;

    public void ShowNotification(string _itemName)
    {
        if (m_notificationCoroutine != null)
        {
            StopCoroutine(m_notificationCoroutine);
        }
        m_notificationCoroutine = StartCoroutine(AnimateNotification(_itemName));
    }

    private IEnumerator AnimateNotification(string _itemName)
    {
        // Set notification text
        m_notificationText.text = $"'{_itemName}' added to the inventory";

        // Slide in
        yield return StartCoroutine(SlideNotification(m_notificationHideX, m_notificationShowX));

        // Wait
        yield return new WaitForSeconds(m_notificationDuration);

        // Slide out
        yield return StartCoroutine(SlideNotification(m_notificationShowX, m_notificationHideX));

        m_notificationCoroutine = null;
    }

    private IEnumerator SlideNotification(float _startX, float _endX)
    {
        float elapsedTime = 0f;
        Vector2 startPos = new Vector2(_startX, m_notificationRect.anchoredPosition.y);
        Vector2 endPos = new Vector2(_endX, m_notificationRect.anchoredPosition.y);

        while (elapsedTime < m_slideDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / m_slideDuration;
            t = t * t * (3f - 2f * t); // Ease in-out cubic
            m_notificationRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        m_notificationRect.anchoredPosition = endPos;
    }
} 