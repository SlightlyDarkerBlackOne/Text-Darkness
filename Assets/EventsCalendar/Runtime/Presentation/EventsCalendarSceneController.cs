using System;
using System.Collections;
using System.Collections.Generic;
using EventsCalendar.Application;
using EventsCalendar.Domain;
using EventsCalendar.Infrastructure.GoogleCalendar;
using EventsCalendar.Infrastructure.Llm;
using EventsCalendar.Infrastructure.Ticketmaster;
using EventsCalendar.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EventsCalendar.Presentation
{
    /// <summary>
    /// Coordinates the landing and calendar importer UI flow for searching music events and importing them into Google Calendar.
    /// </summary>
    public sealed class EventsCalendarSceneController : MonoBehaviour
    {
        [SerializeField] private Button m_importButton;
        [SerializeField] private TMP_InputField m_startDateInput;
        [SerializeField] private TMP_InputField m_endDateInput;
        [SerializeField] private TMP_InputField m_musicStyleInput;
        [SerializeField] private TMP_InputField m_searchSourceInput;
        [SerializeField] private TextMeshProUGUI m_statusText;
        [SerializeField] private string m_ticketmasterApiKey;
        [SerializeField] private string m_llmApiKey;
        [SerializeField] private string m_llmModel = EventCalendarConstants.LargeLanguageModel.DefaultModel;
        [SerializeField] private string m_llmEndpoint = EventCalendarConstants.LargeLanguageModel.DefaultChatCompletionsEndpoint;
        [SerializeField] private string m_googleAccessToken;
        [SerializeField] private string m_googleCalendarId = EventCalendarConstants.GoogleCalendar.PrimaryCalendarId;
        [SerializeField] private int m_maxEvents = EventCalendarConstants.DefaultMaxEvents;
        [SerializeField] private bool m_createUiOnStart = true;
        [SerializeField] private GameObject m_landingPanel;
        [SerializeField] private GameObject m_calendarPanel;
        [SerializeField] private Button m_startSubscriptionButton;
        [SerializeField] private Button m_openCalendarButton;
        [SerializeField] private Button m_backButton;

        private IEventSearchProvider m_eventSearchProvider;
        private ICalendarEventImporter m_calendarEventImporter;
        private bool m_isImporting;

        private static readonly Color s_backgroundColor = new Color(0.025f, 0.03f, 0.055f, 0.98f);
        private static readonly Color s_cardColor = new Color(0.075f, 0.085f, 0.13f, 0.96f);
        private static readonly Color s_elevatedCardColor = new Color(0.105f, 0.12f, 0.18f, 0.98f);
        private static readonly Color s_primaryColor = new Color(0.42f, 0.34f, 1f, 1f);
        private static readonly Color s_secondaryColor = new Color(0.08f, 0.78f, 0.68f, 1f);
        private static readonly Color s_textColor = new Color(0.94f, 0.96f, 1f, 1f);
        private static readonly Color s_mutedTextColor = new Color(0.66f, 0.72f, 0.84f, 1f);
        private static readonly Color s_inputColor = new Color(0.96f, 0.97f, 1f, 1f);

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
                m_eventSearchProvider = CreateEventSearchProvider(EventSearchSource.Ticketmaster);
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

            ConfigureDefaultFields();
            RegisterButtonHandlers();
            ShowLandingPage();
            SetStatus(EventCalendarConstants.Status.Ready);
        }

        private void OnDestroy()
        {
            RemoveButtonHandlers();
        }

        private bool HasRequiredUi()
        {
            return m_statusText != null &&
                m_importButton != null &&
                m_startDateInput != null &&
                m_endDateInput != null &&
                m_musicStyleInput != null &&
                m_searchSourceInput != null &&
                m_landingPanel != null &&
                m_calendarPanel != null;
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
            IEventSearchProvider searchProvider = CreateEventSearchProvider(ParseSearchSource());
            yield return searchProvider.SearchEvents(
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

        private void ConfigureDefaultFields()
        {
            if (string.IsNullOrWhiteSpace(m_startDateInput.text))
            {
                m_startDateInput.text = DateTime.Today.ToString(EventCalendarConstants.Ui.DateFormat);
            }

            if (string.IsNullOrWhiteSpace(m_endDateInput.text))
            {
                m_endDateInput.text = DateTime.Today.AddDays(EventCalendarConstants.DefaultSearchDays).ToString(EventCalendarConstants.Ui.DateFormat);
            }

            if (string.IsNullOrWhiteSpace(m_musicStyleInput.text))
            {
                m_musicStyleInput.text = EventCalendarConstants.MusicStyles.All;
            }

            if (string.IsNullOrWhiteSpace(m_searchSourceInput.text))
            {
                m_searchSourceInput.text = EventCalendarConstants.SearchSources.Ticketmaster;
            }
        }

        private EventSearchSource ParseSearchSource()
        {
            return string.Equals(
                m_searchSourceInput.text,
                EventCalendarConstants.SearchSources.Llm,
                StringComparison.OrdinalIgnoreCase)
                ? EventSearchSource.Llm
                : EventSearchSource.Ticketmaster;
        }

        private IEventSearchProvider CreateEventSearchProvider(EventSearchSource searchSource)
        {
            return searchSource == EventSearchSource.Llm
                ? new LlmEventSearchProvider(m_llmApiKey, m_llmEndpoint, m_llmModel)
                : new TicketmasterEventSearchProvider(m_ticketmasterApiKey);
        }

        private void RegisterButtonHandlers()
        {
            m_importButton.onClick.AddListener(ImportSelectedEvents);

            if (m_startSubscriptionButton != null)
            {
                m_startSubscriptionButton.onClick.AddListener(ShowCalendarPage);
            }

            if (m_openCalendarButton != null)
            {
                m_openCalendarButton.onClick.AddListener(ShowCalendarPage);
            }

            if (m_backButton != null)
            {
                m_backButton.onClick.AddListener(ShowLandingPage);
            }
        }

        private void RemoveButtonHandlers()
        {
            if (m_importButton != null)
            {
                m_importButton.onClick.RemoveListener(ImportSelectedEvents);
            }

            if (m_startSubscriptionButton != null)
            {
                m_startSubscriptionButton.onClick.RemoveListener(ShowCalendarPage);
            }

            if (m_openCalendarButton != null)
            {
                m_openCalendarButton.onClick.RemoveListener(ShowCalendarPage);
            }

            if (m_backButton != null)
            {
                m_backButton.onClick.RemoveListener(ShowLandingPage);
            }
        }

        private void ShowLandingPage()
        {
            m_landingPanel.SetActive(true);
            m_calendarPanel.SetActive(false);
        }

        private void ShowCalendarPage()
        {
            m_landingPanel.SetActive(false);
            m_calendarPanel.SetActive(true);
        }

        private void EnsureUi()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(EventCalendarConstants.Ui.CanvasName, typeof(RectTransform));
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (HasRequiredUi())
            {
                return;
            }

            GameObject root = CreateFullScreenObject(EventCalendarConstants.Ui.RootName, canvas.transform);
            AddImage(root, s_backgroundColor);
            m_landingPanel = CreateLandingPanel(root.transform);
            m_calendarPanel = CreateCalendarPanel(root.transform);
        }

        private GameObject CreateLandingPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenObject(EventCalendarConstants.Ui.LandingPanelName, parent);
            GameObject card = CreateCard(panel.transform, new Vector2(0f, 0f), new Vector2(860f, 560f));
            CreateAccent(card.transform, new Vector2(-410f, 0f), new Vector2(8f, 500f), s_secondaryColor);
            CreateText(card.transform, EventCalendarConstants.Ui.HeroEyebrow, new Vector2(-180f, 200f), new Vector2(420f, 34f), 18, s_secondaryColor, TextAlignmentOptions.Left);
            CreateText(card.transform, EventCalendarConstants.Ui.LandingTitle, new Vector2(-120f, 130f), new Vector2(540f, 96f), 44, s_textColor, TextAlignmentOptions.Left);
            CreateText(card.transform, EventCalendarConstants.Ui.LandingSubtitle, new Vector2(-120f, 28f), new Vector2(560f, 92f), 20, s_mutedTextColor, TextAlignmentOptions.Left);

            GameObject priceCard = CreateCard(card.transform, new Vector2(245f, 82f), new Vector2(250f, 220f), s_elevatedCardColor);
            CreateText(priceCard.transform, EventCalendarConstants.Ui.SubscriptionLabel, new Vector2(0f, 66f), new Vector2(210f, 30f), 18, s_mutedTextColor, TextAlignmentOptions.Center);
            CreateText(priceCard.transform, EventCalendarConstants.Ui.SubscriptionPrice, new Vector2(0f, 8f), new Vector2(220f, 62f), 38, s_textColor, TextAlignmentOptions.Center);
            CreateText(priceCard.transform, EventCalendarConstants.Ui.SubscriptionPeriod, new Vector2(0f, -42f), new Vector2(210f, 28f), 16, s_mutedTextColor, TextAlignmentOptions.Center);

            m_startSubscriptionButton = CreateButton(card.transform, EventCalendarConstants.Ui.StartSubscriptionButtonName, EventCalendarConstants.Ui.PrimaryCallToAction, new Vector2(-210f, -190f), new Vector2(230f, 54f), s_primaryColor);
            m_openCalendarButton = CreateButton(card.transform, EventCalendarConstants.Ui.OpenCalendarButtonName, EventCalendarConstants.Ui.SecondaryCallToAction, new Vector2(55f, -190f), new Vector2(260f, 54f), s_elevatedCardColor);
            return panel;
        }

        private GameObject CreateCalendarPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenObject(EventCalendarConstants.Ui.CalendarPanelName, parent);
            GameObject card = CreateCard(panel.transform, new Vector2(0f, 0f), new Vector2(760f, 560f));
            CreateAccent(card.transform, new Vector2(0f, 258f), new Vector2(680f, 5f), s_primaryColor);
            CreateText(card.transform, EventCalendarConstants.Ui.CalendarTitle, new Vector2(-120f, 198f), new Vector2(470f, 54f), 34, s_textColor, TextAlignmentOptions.Left);
            CreateText(card.transform, EventCalendarConstants.Ui.CalendarSubtitle, new Vector2(-60f, 142f), new Vector2(590f, 58f), 18, s_mutedTextColor, TextAlignmentOptions.Left);

            m_startDateInput = CreateInput(card.transform, EventCalendarConstants.Ui.StartDateLabel, EventCalendarConstants.Ui.DatePlaceholder, new Vector2(-170f, 56f));
            m_endDateInput = CreateInput(card.transform, EventCalendarConstants.Ui.EndDateLabel, EventCalendarConstants.Ui.DatePlaceholder, new Vector2(170f, 56f));
            m_musicStyleInput = CreateInput(card.transform, EventCalendarConstants.Ui.StyleLabel, EventCalendarConstants.Ui.StylePlaceholder, new Vector2(-170f, -32f));
            m_searchSourceInput = CreateInput(card.transform, EventCalendarConstants.Ui.SearchSourceLabel, EventCalendarConstants.Ui.SearchSourcePlaceholder, new Vector2(170f, -32f));
            m_importButton = CreateButton(card.transform, EventCalendarConstants.Ui.ImportButtonName, EventCalendarConstants.Ui.ImportButtonText, new Vector2(0f, -126f), new Vector2(340f, 54f), s_primaryColor);
            m_backButton = CreateButton(card.transform, EventCalendarConstants.Ui.BackButtonName, EventCalendarConstants.Ui.BackButtonText, new Vector2(-290f, -218f), new Vector2(120f, 42f), s_elevatedCardColor);
            m_statusText = CreateText(card.transform, EventCalendarConstants.Status.Ready, new Vector2(80f, -218f), new Vector2(500f, 46f), 17, s_mutedTextColor, TextAlignmentOptions.Left);
            return panel;
        }

        private GameObject CreateFullScreenObject(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return gameObject;
        }

        private GameObject CreateCard(Transform parent, Vector2 position, Vector2 size)
        {
            return CreateCard(parent, position, size, s_cardColor);
        }

        private GameObject CreateCard(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            GameObject card = new GameObject(EventCalendarConstants.Ui.CardName, typeof(RectTransform));
            card.transform.SetParent(parent, false);
            AddImage(card, color);

            RectTransform rectTransform = card.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            return card;
        }

        private void CreateAccent(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            GameObject accent = new GameObject(EventCalendarConstants.Ui.AccentName, typeof(RectTransform));
            accent.transform.SetParent(parent, false);
            AddImage(accent, color);

            RectTransform rectTransform = accent.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;
        }

        private TMP_InputField CreateInput(Transform parent, string label, string placeholderText, Vector2 position)
        {
            CreateText(parent, label, position + new Vector2(-92f, 34f), new Vector2(240f, 26f), 15, s_mutedTextColor, TextAlignmentOptions.Left);

            GameObject inputObject = new GameObject(label, typeof(RectTransform));
            inputObject.transform.SetParent(parent, false);
            AddImage(inputObject, s_inputColor);

            TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();
            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchoredPosition = position;
            inputRect.sizeDelta = new Vector2(280f, 42f);

            TextMeshProUGUI text = CreateText(inputObject.transform, string.Empty, Vector2.zero, new Vector2(248f, 34f), 17, Color.black, TextAlignmentOptions.MidlineLeft);
            text.gameObject.name = EventCalendarConstants.Ui.InputTextObjectName;
            inputField.textComponent = text;

            TextMeshProUGUI placeholder = CreateText(inputObject.transform, placeholderText, Vector2.zero, new Vector2(248f, 34f), 17, new Color(0.43f, 0.45f, 0.54f, 1f), TextAlignmentOptions.MidlineLeft);
            placeholder.gameObject.name = EventCalendarConstants.Ui.PlaceholderObjectName;
            inputField.placeholder = placeholder;

            return inputField;
        }

        private Button CreateButton(Transform parent, string objectName, string labelValue, Vector2 position, Vector2 size, Color color)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            AddImage(buttonObject, color);

            Button button = buttonObject.AddComponent<Button>();
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            CreateText(buttonObject.transform, labelValue, Vector2.zero, size, 18, s_textColor, TextAlignmentOptions.Center);
            return button;
        }

        private TextMeshProUGUI CreateText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(EventCalendarConstants.Ui.TextObjectName, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            return text;
        }

        private void AddImage(GameObject gameObject, Color color)
        {
            Image image = gameObject.AddComponent<Image>();
            image.color = color;
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
