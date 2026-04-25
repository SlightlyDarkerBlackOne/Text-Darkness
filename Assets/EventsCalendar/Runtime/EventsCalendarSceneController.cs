using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EventsCalendar.Runtime
{
    /// <summary>
    /// Coordinates the scene UI action that searches music events and imports them into Google Calendar.
    /// </summary>
    public sealed class EventsCalendarSceneController : MonoBehaviour
    {
        [SerializeField] private Button m_importButton;
        [SerializeField] private TMP_InputField m_startDateInput;
        [SerializeField] private TMP_InputField m_endDateInput;
        [SerializeField] private TMP_InputField m_musicStyleInput;
        [SerializeField] private TextMeshProUGUI m_statusText;
        [SerializeField] private string m_ticketmasterApiKey;
        [SerializeField] private string m_googleAccessToken;
        [SerializeField] private string m_googleCalendarId = EventCalendarConstants.GoogleCalendar.PrimaryCalendarId;
        [SerializeField] private int m_maxEvents = EventCalendarConstants.DefaultMaxEvents;
        [SerializeField] private bool m_createUiOnStart = true;

        private IEventSearchProvider m_eventSearchProvider;
        private ICalendarEventImporter m_calendarEventImporter;
        private bool m_isImporting;

        /// <summary>
        /// Injects event search and calendar import services for tests or custom scene composition.
        /// </summary>
        public void Initialize(IEventSearchProvider eventSearchProvider, ICalendarEventImporter calendarEventImporter)
        {
            m_eventSearchProvider = eventSearchProvider;
            m_calendarEventImporter = calendarEventImporter;
        }

        private void Awake()
        {
            if (m_eventSearchProvider == null)
            {
                m_eventSearchProvider = new TicketmasterEventSearchProvider(m_ticketmasterApiKey);
            }

            if (m_calendarEventImporter == null)
            {
                m_calendarEventImporter = new GoogleCalendarEventImporter(m_googleAccessToken, m_googleCalendarId);
            }
        }

        private void Start()
        {
            if (m_createUiOnStart)
            {
                EnsureUi();
            }

            if (!HasRequiredUi())
            {
                Debug.LogError(EventCalendarConstants.Messages.RequiredUiMissing);
                enabled = false;
                return;
            }

            ConfigureDefaultDates();
            m_importButton.onClick.AddListener(ImportSelectedEvents);
            SetStatus(EventCalendarConstants.Status.Ready);
        }

        private void OnDestroy()
        {
            if (m_importButton != null)
            {
                m_importButton.onClick.RemoveListener(ImportSelectedEvents);
            }
        }

        private bool HasRequiredUi()
        {
            return m_statusText != null &&
                m_importButton != null &&
                m_startDateInput != null &&
                m_endDateInput != null &&
                m_musicStyleInput != null;
        }

        /// <summary>
        /// Starts importing events for the period and style currently selected in the scene UI.
        /// </summary>
        public void ImportSelectedEvents()
        {
            if (m_isImporting)
            {
                return;
            }

            if (!TryCreateFilter(out EventSearchFilter filter, out string validationMessage))
            {
                SetStatus(validationMessage);
                return;
            }

            StartCoroutine(ImportSelectedEventsCoroutine(filter));
        }

        private IEnumerator ImportSelectedEventsCoroutine(EventSearchFilter filter)
        {
            m_isImporting = true;
            m_importButton.interactable = false;
            SetStatus(EventCalendarConstants.Status.Searching);

            IReadOnlyList<MusicEvent> foundEvents = Array.Empty<MusicEvent>();
            string searchError = string.Empty;
            yield return m_eventSearchProvider.SearchEvents(
                filter,
                events => foundEvents = events,
                errorMessage => searchError = errorMessage);

            if (!string.IsNullOrWhiteSpace(searchError))
            {
                SetStatus(string.Format(EventCalendarConstants.Status.SearchFailedFormat, searchError));
                ResetImportState();
                yield break;
            }

            if (foundEvents.Count == 0)
            {
                SetStatus(EventCalendarConstants.Status.NoEventsFound);
                ResetImportState();
                yield break;
            }

            int importedCount = 0;
            int failedCount = 0;
            SetStatus(string.Format(EventCalendarConstants.Status.ImportingFormat, foundEvents.Count));

            foreach (MusicEvent musicEvent in foundEvents)
            {
                EventImportResult importResult = null;
                yield return m_calendarEventImporter.ImportAsync(musicEvent, result => importResult = result);

                if (importResult != null && importResult.IsSuccessful)
                {
                    importedCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            SetStatus(string.Format(EventCalendarConstants.Status.ImportFinishedFormat, importedCount, failedCount));
            ResetImportState();
        }

        private bool TryCreateFilter(out EventSearchFilter filter, out string validationMessage)
        {
            filter = null;

            if (!DateTime.TryParseExact(
                m_startDateInput.text,
                EventCalendarConstants.Ui.DateFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime startDate))
            {
                validationMessage = EventCalendarConstants.Validation.InvalidStartDate;
                return false;
            }

            if (!DateTime.TryParseExact(
                m_endDateInput.text,
                EventCalendarConstants.Ui.DateFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime endDate))
            {
                validationMessage = EventCalendarConstants.Validation.InvalidEndDate;
                return false;
            }

            if (endDate < startDate)
            {
                validationMessage = EventCalendarConstants.Validation.EndBeforeStart;
                return false;
            }

            string selectedStyle = m_musicStyleInput.text;
            string normalizedStyle = string.Equals(
                selectedStyle,
                EventCalendarConstants.MusicStyles.All,
                StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : selectedStyle;
            filter = new EventSearchFilter(startDate, endDate, normalizedStyle, Mathf.Max(1, m_maxEvents));
            validationMessage = string.Empty;
            return true;
        }

        private void ConfigureMusicStyleInput()
        {
            if (string.IsNullOrWhiteSpace(m_musicStyleInput.text))
            {
                m_musicStyleInput.text = EventCalendarConstants.MusicStyles.All;
            }
        }

        private void ConfigureDefaultDates()
        {
            if (string.IsNullOrWhiteSpace(m_startDateInput.text))
            {
                m_startDateInput.text = DateTime.Today.ToString(EventCalendarConstants.Ui.DateFormat);
            }

            if (string.IsNullOrWhiteSpace(m_endDateInput.text))
            {
                m_endDateInput.text = DateTime.Today.AddDays(EventCalendarConstants.DefaultSearchDays).ToString(EventCalendarConstants.Ui.DateFormat);
            }
        }

        private void EnsureUi()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(EventCalendarConstants.Ui.CanvasName);
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (m_statusText != null && m_importButton != null && m_startDateInput != null && m_endDateInput != null && m_musicStyleInput != null)
            {
                return;
            }

            GameObject panel = CreatePanel(canvas.transform);
            m_startDateInput ??= CreateInput(panel.transform, EventCalendarConstants.Ui.StartDateLabel, new Vector2(0f, 90f));
            m_endDateInput ??= CreateInput(panel.transform, EventCalendarConstants.Ui.EndDateLabel, new Vector2(0f, 35f));
            m_musicStyleInput ??= CreateInput(panel.transform, EventCalendarConstants.Ui.StyleLabel, new Vector2(0f, -20f));
            m_importButton ??= CreateButton(panel.transform, new Vector2(0f, -80f));
            m_statusText ??= CreateText(panel.transform, EventCalendarConstants.Status.Ready, new Vector2(0f, -135f), 18);
        }

        private GameObject CreatePanel(Transform parent)
        {
            GameObject panel = new GameObject(EventCalendarConstants.Ui.PanelName);
            panel.transform.SetParent(parent, false);

            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.06f, 0.06f, 0.08f, 0.92f);

            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-24f, -24f);
            rectTransform.sizeDelta = new Vector2(360f, 300f);

            CreateText(panel.transform, EventCalendarConstants.Ui.TitleText, new Vector2(0f, 132f), 24);
            return panel;
        }

        private TMP_InputField CreateInput(Transform parent, string label, Vector2 position)
        {
            CreateText(parent, label, position + new Vector2(-95f, 22f), 15);

            GameObject inputObject = new GameObject(label);
            inputObject.transform.SetParent(parent, false);

            Image image = inputObject.AddComponent<Image>();
            image.color = Color.white;

            TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();
            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchoredPosition = position;
            inputRect.sizeDelta = new Vector2(250f, 34f);

            TextMeshProUGUI text = CreateText(inputObject.transform, string.Empty, Vector2.zero, 18);
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.rectTransform.offsetMin = new Vector2(8f, 0f);
            text.rectTransform.offsetMax = new Vector2(-8f, 0f);
            inputField.textComponent = text;

            TextMeshProUGUI placeholder = CreateText(inputObject.transform, EventCalendarConstants.Ui.DatePlaceholder, Vector2.zero, 18);
            placeholder.color = new Color(0.45f, 0.45f, 0.45f, 1f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.rectTransform.offsetMin = new Vector2(8f, 0f);
            placeholder.rectTransform.offsetMax = new Vector2(-8f, 0f);
            inputField.placeholder = placeholder;

            return inputField;
        }

        private Button CreateButton(Transform parent, Vector2 position)
        {
            GameObject buttonObject = new GameObject(EventCalendarConstants.Ui.ImportButtonName);
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.1f, 0.38f, 0.75f, 1f);

            Button button = buttonObject.AddComponent<Button>();
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(250f, 42f);

            TextMeshProUGUI label = CreateText(buttonObject.transform, EventCalendarConstants.Ui.ImportButtonText, Vector2.zero, 18);
            label.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private TextMeshProUGUI CreateText(Transform parent, string value, Vector2 position, int fontSize)
        {
            GameObject textObject = new GameObject(EventCalendarConstants.Ui.TextObjectName);
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = new Vector2(320f, 34f);

            return text;
        }

        private void ResetImportState()
        {
            if (m_importButton != null)
            {
                m_importButton.interactable = true;
            }

            m_isImporting = false;
        }

        private void SetStatus(string message)
        {
            if (m_statusText != null)
            {
                m_statusText.text = message;
            }
        }
    }
}
