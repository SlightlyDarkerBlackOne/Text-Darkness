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
using UnityEngine.EventSystems;
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
        [SerializeField] private Button m_musicStyleDropdownButton;
        [SerializeField] private TextMeshProUGUI m_musicStyleDropdownLabel;
        [SerializeField] private GameObject m_musicStyleOptionsPanel;
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
        [SerializeField] private Button m_retryFailedButton;
        [SerializeField] private GameObject m_previewPanel;
        [SerializeField] private GameObject m_previewContent;
        [SerializeField] private TextMeshProUGUI m_previewTitleText;
        [SerializeField] private TextMeshProUGUI m_previewSummaryText;

        private IEventSearchProvider m_eventSearchProvider;
        private ICalendarEventImporter m_calendarEventImporter;
        private readonly List<Toggle> m_musicStyleToggles = new List<Toggle>();
        private readonly List<EventPreviewItem> m_previewItems = new List<EventPreviewItem>();
        private readonly List<MusicEvent> m_failedImportEvents = new List<MusicEvent>();
        private readonly HashSet<string> m_importedEventKeys = new HashSet<string>();
        private bool m_isImporting;
        private bool m_hasPreviewResults;

        private static readonly Color s_backgroundColor = new Color(0.018f, 0.023f, 0.04f, 1f);
        private static readonly Color s_cardColor = new Color(0.07f, 0.08f, 0.12f, 0.97f);
        private static readonly Color s_elevatedCardColor = new Color(0.105f, 0.12f, 0.18f, 0.98f);
        private static readonly Color s_primaryColor = new Color(0.44f, 0.36f, 1f, 1f);
        private static readonly Color s_secondaryColor = new Color(0.08f, 0.82f, 0.74f, 1f);
        private static readonly Color s_textColor = new Color(0.94f, 0.96f, 1f, 1f);
        private static readonly Color s_mutedTextColor = new Color(0.66f, 0.72f, 0.84f, 1f);
        private static readonly Color s_inputColor = new Color(0.96f, 0.97f, 1f, 1f);
        private static readonly Color s_panelColor = new Color(0.04f, 0.05f, 0.08f, 0.98f);
        private static readonly Color s_toggleOffColor = new Color(0.16f, 0.18f, 0.24f, 1f);
        private static readonly Vector2 s_shellSize = new Vector2(1320f, 760f);
        private static readonly Vector2 s_inputSize = new Vector2(300f, 46f);

        private sealed class EventPreviewItem
        {
            public EventPreviewItem(MusicEvent musicEvent, Toggle selectionToggle, TextMeshProUGUI statusText, string eventKey)
            {
                MusicEvent = musicEvent;
                SelectionToggle = selectionToggle;
                StatusText = statusText;
                EventKey = eventKey;
            }

            public MusicEvent MusicEvent { get; }
            public Toggle SelectionToggle { get; }
            public TextMeshProUGUI StatusText { get; }
            public string EventKey { get; }
        }

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
                m_musicStyleDropdownButton != null &&
                m_musicStyleDropdownLabel != null &&
                m_musicStyleOptionsPanel != null &&
                m_searchSourceInput != null &&
                m_previewPanel != null &&
                m_previewContent != null &&
                m_previewTitleText != null &&
                m_previewSummaryText != null &&
                m_retryFailedButton != null &&
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

            if (m_hasPreviewResults)
            {
                StartCoroutine(ImportPreviewedEventsCoroutine(GetSelectedPreviewItems()));
                return;
            }

            if (!TryCreateFilter(out EventSearchFilter filter, out string validationMessage))
            {
                SetStatus(validationMessage);
                return;
            }

            StartCoroutine(SearchSelectedEventsCoroutine(filter));
        }

        private IEnumerator SearchSelectedEventsCoroutine(EventSearchFilter filter)
        {
            m_isImporting = true;
            m_importButton.interactable = false;
            m_retryFailedButton.interactable = false;
            ClearPreviewItems();
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
                SetPreviewSummary(EventCalendarConstants.Ui.PreviewEmptyText);
                ResetImportState();
                yield break;
            }

            PopulatePreview(foundEvents);
            SetStatus(string.Format(EventCalendarConstants.Status.PreviewReadyFormat, foundEvents.Count));
            ResetImportState();
        }

        private IEnumerator ImportPreviewedEventsCoroutine(IReadOnlyList<EventPreviewItem> selectedItems)
        {
            if (selectedItems.Count == 0)
            {
                SetStatus(EventCalendarConstants.Status.NoEventsSelected);
                yield break;
            }

            m_isImporting = true;
            m_importButton.interactable = false;
            m_retryFailedButton.interactable = false;
            SetStatus(string.Format(EventCalendarConstants.Status.ImportingFormat, selectedItems.Count));

            int importedCount = 0;
            int failedCount = 0;
            int duplicateCount = 0;
            m_failedImportEvents.Clear();

            foreach (EventPreviewItem previewItem in selectedItems)
            {
                if (m_importedEventKeys.Contains(previewItem.EventKey))
                {
                    duplicateCount++;
                    SetPreviewItemStatus(previewItem, EventCalendarConstants.Ui.DuplicateBadge);
                    continue;
                }

                EventImportResult importResult = null;
                yield return m_calendarEventImporter.ImportAsync(previewItem.MusicEvent, result => importResult = result);

                if (importResult != null && importResult.IsSuccessful)
                {
                    importedCount++;
                    m_importedEventKeys.Add(previewItem.EventKey);
                    previewItem.SelectionToggle.isOn = false;
                    previewItem.SelectionToggle.interactable = false;
                    SetPreviewItemStatus(previewItem, EventCalendarConstants.Ui.ImportedBadge);
                }
                else
                {
                    failedCount++;
                    m_failedImportEvents.Add(previewItem.MusicEvent);
                    SetPreviewItemStatus(previewItem, EventCalendarConstants.Ui.FailedBadge);
                }
            }

            SetStatus(string.Format(EventCalendarConstants.Status.ImportFinishedFormat, importedCount, failedCount, duplicateCount));
            m_retryFailedButton.interactable = m_failedImportEvents.Count > 0;
            UpdatePreviewSummary();
            ResetImportState();
        }

        private void RetryFailedImports()
        {
            if (m_isImporting || m_failedImportEvents.Count == 0)
            {
                return;
            }

            PopulatePreview(m_failedImportEvents);
            SetStatus(string.Format(EventCalendarConstants.Status.RetryReadyFormat, m_failedImportEvents.Count));
        }

        private void PopulatePreview(IReadOnlyList<MusicEvent> musicEvents)
        {
            ClearPreviewItems();
            ResizePreviewContent(musicEvents.Count);
            HashSet<string> visibleKeys = new HashSet<string>();

            for (int index = 0; index < musicEvents.Count; index++)
            {
                MusicEvent musicEvent = musicEvents[index];
                string eventKey = CreateEventKey(musicEvent);
                bool isDuplicate = m_importedEventKeys.Contains(eventKey) || !visibleKeys.Add(eventKey);
                EventPreviewItem previewItem = CreatePreviewItem(musicEvent, eventKey, index, isDuplicate);
                m_previewItems.Add(previewItem);
            }

            m_hasPreviewResults = m_previewItems.Count > 0;
            SetButtonLabel(m_importButton, m_hasPreviewResults ? EventCalendarConstants.Ui.ImportButtonText : EventCalendarConstants.Ui.SearchButtonText);
            UpdatePreviewSummary();
        }

        private EventPreviewItem CreatePreviewItem(MusicEvent musicEvent, string eventKey, int index, bool isDuplicate)
        {
            GameObject row = CreateCard(m_previewContent.transform, Vector2.zero, new Vector2(470f, 56f), s_elevatedCardColor, EventCalendarConstants.Ui.PreviewRowName);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -index * 64f);
            Toggle selectionToggle = CreatePreviewToggle(row.transform, new Vector2(-220f, 0f));
            TextMeshProUGUI rowText = CreateText(
                row.transform,
                string.Format(EventCalendarConstants.Ui.PreviewRowFormat, musicEvent.Title, musicEvent.VenueName, musicEvent.DurationText),
                new Vector2(-10f, 0f),
                new Vector2(360f, 46f),
                13,
                s_textColor,
                TextAlignmentOptions.MidlineLeft);
            TextMeshProUGUI statusText = CreateText(row.transform, string.Empty, new Vector2(198f, 0f), new Vector2(82f, 26f), 12, s_secondaryColor, TextAlignmentOptions.Center);

            selectionToggle.isOn = !isDuplicate;
            selectionToggle.interactable = !isDuplicate;
            selectionToggle.onValueChanged.AddListener(_ => UpdatePreviewSummary());

            EventPreviewItem previewItem = new EventPreviewItem(musicEvent, selectionToggle, statusText, eventKey);
            if (isDuplicate)
            {
                SetPreviewItemStatus(previewItem, EventCalendarConstants.Ui.DuplicateBadge);
            }

            rowText.textWrappingMode = TextWrappingModes.NoWrap;
            rowText.overflowMode = TextOverflowModes.Ellipsis;
            return previewItem;
        }

        private Toggle CreatePreviewToggle(Transform parent, Vector2 position)
        {
            GameObject toggleObject = new GameObject(EventCalendarConstants.Ui.PreviewRowName, typeof(RectTransform));
            toggleObject.transform.SetParent(parent, false);

            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.anchoredPosition = position;
            toggleRect.sizeDelta = new Vector2(24f, 24f);

            Toggle toggle = toggleObject.AddComponent<Toggle>();
            GameObject background = CreateCard(toggleObject.transform, Vector2.zero, new Vector2(20f, 20f), s_toggleOffColor, EventCalendarConstants.Ui.CardName);
            GameObject checkmark = CreateCard(background.transform, Vector2.zero, new Vector2(12f, 12f), s_secondaryColor, EventCalendarConstants.Ui.CheckmarkObjectName);
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.targetGraphic = background.GetComponent<Image>();
            return toggle;
        }

        private IReadOnlyList<EventPreviewItem> GetSelectedPreviewItems()
        {
            List<EventPreviewItem> selectedItems = new List<EventPreviewItem>();
            foreach (EventPreviewItem previewItem in m_previewItems)
            {
                if (previewItem.SelectionToggle.isOn && previewItem.SelectionToggle.interactable)
                {
                    selectedItems.Add(previewItem);
                }
            }

            return selectedItems;
        }

        private void ClearPreviewItems()
        {
            foreach (EventPreviewItem previewItem in m_previewItems)
            {
                if (previewItem.SelectionToggle != null)
                {
                    previewItem.SelectionToggle.onValueChanged.RemoveAllListeners();
                    Destroy(previewItem.SelectionToggle.transform.parent.gameObject);
                }
            }

            m_previewItems.Clear();
            m_hasPreviewResults = false;
            SetButtonLabel(m_importButton, EventCalendarConstants.Ui.SearchButtonText);
            ResizePreviewContent(0);
            SetPreviewSummary(EventCalendarConstants.Ui.PreviewEmptyText);
        }

        private void ResizePreviewContent(int itemCount)
        {
            if (m_previewContent == null)
            {
                return;
            }

            RectTransform contentRect = m_previewContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(0f, Mathf.Max(300f, itemCount * 64f));
            contentRect.anchoredPosition = Vector2.zero;
        }

        private void UpdatePreviewSummary()
        {
            int selectedCount = GetSelectedPreviewItems().Count;
            SetPreviewSummary(string.Format(EventCalendarConstants.Ui.SelectedCountFormat, selectedCount));
        }

        private void SetPreviewSummary(string message)
        {
            if (m_previewSummaryText != null)
            {
                m_previewSummaryText.text = message;
            }
        }

        private void SetPreviewItemStatus(EventPreviewItem previewItem, string status)
        {
            if (previewItem.StatusText != null)
            {
                previewItem.StatusText.text = status;
            }
        }

        private string CreateEventKey(MusicEvent musicEvent)
        {
            if (!string.IsNullOrWhiteSpace(musicEvent.ExternalId))
            {
                return musicEvent.ExternalId.Trim();
            }

            return string.Join(
                EventCalendarConstants.Search.MultipleStylesSeparator,
                musicEvent.Title,
                musicEvent.VenueName,
                musicEvent.StartUtc.ToUniversalTime().ToString(EventCalendarConstants.GoogleCalendar.DateTimeFormat));
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

            string selectedStyle = GetSelectedMusicStyleFilter();
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

            ConfigureDefaultMusicStyleSelection();

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
            m_startDateInput.onValueChanged.AddListener(HandleSearchCriteriaChanged);
            m_endDateInput.onValueChanged.AddListener(HandleSearchCriteriaChanged);
            m_searchSourceInput.onValueChanged.AddListener(HandleSearchCriteriaChanged);

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

            if (m_musicStyleDropdownButton != null)
            {
                m_musicStyleDropdownButton.onClick.AddListener(ToggleMusicStyleOptions);
            }

            if (m_retryFailedButton != null)
            {
                m_retryFailedButton.onClick.AddListener(RetryFailedImports);
            }

            foreach (Toggle styleToggle in m_musicStyleToggles)
            {
                styleToggle.onValueChanged.AddListener(HandleMusicStyleSelectionChanged);
            }
        }

        private void RemoveButtonHandlers()
        {
            if (m_importButton != null)
            {
                m_importButton.onClick.RemoveListener(ImportSelectedEvents);
            }

            if (m_startDateInput != null)
            {
                m_startDateInput.onValueChanged.RemoveListener(HandleSearchCriteriaChanged);
            }

            if (m_endDateInput != null)
            {
                m_endDateInput.onValueChanged.RemoveListener(HandleSearchCriteriaChanged);
            }

            if (m_searchSourceInput != null)
            {
                m_searchSourceInput.onValueChanged.RemoveListener(HandleSearchCriteriaChanged);
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

            if (m_musicStyleDropdownButton != null)
            {
                m_musicStyleDropdownButton.onClick.RemoveListener(ToggleMusicStyleOptions);
            }

            if (m_retryFailedButton != null)
            {
                m_retryFailedButton.onClick.RemoveListener(RetryFailedImports);
            }

            foreach (Toggle styleToggle in m_musicStyleToggles)
            {
                styleToggle.onValueChanged.RemoveListener(HandleMusicStyleSelectionChanged);
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
            EnsureEventSystem();
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject(EventCalendarConstants.Ui.CanvasName, typeof(RectTransform));
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
                ConfigureCanvasScaler(canvasScaler);
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
                if (canvasScaler != null)
                {
                    ConfigureCanvasScaler(canvasScaler);
                }
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

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject(EventCalendarConstants.Ui.EventSystemName);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private GameObject CreateLandingPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenObject(EventCalendarConstants.Ui.LandingPanelName, parent);
            GameObject shell = CreateCard(panel.transform, new Vector2(0f, 0f), s_shellSize, s_panelColor, EventCalendarConstants.Ui.ShellName);
            CreateAccent(shell.transform, new Vector2(-610f, 0f), new Vector2(6f, 620f), s_secondaryColor);

            GameObject heroCard = CreateCard(shell.transform, new Vector2(-300f, 40f), new Vector2(560f, 520f), s_cardColor, EventCalendarConstants.Ui.HeroCardName);
            CreateText(heroCard.transform, EventCalendarConstants.Ui.HeroEyebrow, new Vector2(-190f, 182f), new Vector2(340f, 28f), 15, s_secondaryColor, TextAlignmentOptions.Left);
            CreateText(heroCard.transform, EventCalendarConstants.Ui.LandingTitle, new Vector2(-100f, 92f), new Vector2(420f, 118f), 42, s_textColor, TextAlignmentOptions.Left);
            CreateText(heroCard.transform, EventCalendarConstants.Ui.LandingSubtitle, new Vector2(-70f, -42f), new Vector2(440f, 92f), 17, s_mutedTextColor, TextAlignmentOptions.Left);

            GameObject featureCard = CreateCard(shell.transform, new Vector2(350f, 86f), new Vector2(360f, 280f), s_elevatedCardColor, EventCalendarConstants.Ui.FormCardName);
            CreateText(featureCard.transform, EventCalendarConstants.Ui.LandingFeatureTitle, new Vector2(0f, 78f), new Vector2(300f, 38f), 25, s_textColor, TextAlignmentOptions.Center);
            CreateText(featureCard.transform, EventCalendarConstants.Ui.LandingFeatureDescription, new Vector2(0f, 10f), new Vector2(290f, 78f), 15, s_mutedTextColor, TextAlignmentOptions.Center);
            CreateText(featureCard.transform, EventCalendarConstants.Ui.SubscriptionLabel, new Vector2(0f, -58f), new Vector2(200f, 24f), 14, s_mutedTextColor, TextAlignmentOptions.Center);
            CreateText(featureCard.transform, EventCalendarConstants.Ui.SubscriptionPrice, new Vector2(0f, -98f), new Vector2(220f, 44f), 30, s_textColor, TextAlignmentOptions.Center);
            CreateText(featureCard.transform, EventCalendarConstants.Ui.SubscriptionPeriod, new Vector2(0f, -132f), new Vector2(200f, 22f), 13, s_mutedTextColor, TextAlignmentOptions.Center);

            m_startSubscriptionButton = CreateButton(shell.transform, EventCalendarConstants.Ui.StartSubscriptionButtonName, EventCalendarConstants.Ui.PrimaryCallToAction, new Vector2(225f, -235f), new Vector2(250f, 54f), s_primaryColor);
            m_openCalendarButton = CreateButton(shell.transform, EventCalendarConstants.Ui.OpenCalendarButtonName, EventCalendarConstants.Ui.SecondaryCallToAction, new Vector2(500f, -235f), new Vector2(260f, 54f), s_elevatedCardColor);
            return panel;
        }

        private GameObject CreateCalendarPanel(Transform parent)
        {
            GameObject panel = CreateFullScreenObject(EventCalendarConstants.Ui.CalendarPanelName, parent);
            GameObject shell = CreateCard(panel.transform, new Vector2(0f, 0f), s_shellSize, s_panelColor, EventCalendarConstants.Ui.ShellName);
            CreateAccent(shell.transform, new Vector2(0f, 344f), new Vector2(1180f, 5f), s_primaryColor);

            CreateText(shell.transform, EventCalendarConstants.Ui.CalendarTitle, new Vector2(-330f, 270f), new Vector2(520f, 56f), 34, s_textColor, TextAlignmentOptions.Left);
            CreateText(shell.transform, EventCalendarConstants.Ui.CalendarSubtitle, new Vector2(-270f, 220f), new Vector2(650f, 46f), 16, s_mutedTextColor, TextAlignmentOptions.Left);

            GameObject formCard = CreateCard(shell.transform, new Vector2(-315f, -34f), new Vector2(560f, 480f), s_cardColor, EventCalendarConstants.Ui.FormCardName);
            m_startDateInput = CreateInput(formCard.transform, EventCalendarConstants.Ui.StartDateLabel, EventCalendarConstants.Ui.DatePlaceholder, new Vector2(0f, 156f), s_inputSize);
            m_endDateInput = CreateInput(formCard.transform, EventCalendarConstants.Ui.EndDateLabel, EventCalendarConstants.Ui.DatePlaceholder, new Vector2(0f, 74f), s_inputSize);
            CreateMusicStyleDropdown(formCard.transform, new Vector2(0f, -8f), s_inputSize);
            m_searchSourceInput = CreateInput(formCard.transform, EventCalendarConstants.Ui.SearchSourceLabel, EventCalendarConstants.Ui.SearchSourcePlaceholder, new Vector2(0f, -90f), s_inputSize);
            m_importButton = CreateButton(formCard.transform, EventCalendarConstants.Ui.ImportButtonName, EventCalendarConstants.Ui.SearchButtonText, new Vector2(85f, -180f), new Vector2(230f, 52f), s_primaryColor);
            m_backButton = CreateButton(formCard.transform, EventCalendarConstants.Ui.BackButtonName, EventCalendarConstants.Ui.BackButtonText, new Vector2(-170f, -180f), new Vector2(120f, 46f), s_elevatedCardColor);

            m_previewPanel = CreateCard(shell.transform, new Vector2(350f, -34f), new Vector2(560f, 480f), s_cardColor, EventCalendarConstants.Ui.PreviewPanelName);
            m_previewTitleText = CreateText(m_previewPanel.transform, EventCalendarConstants.Ui.PreviewTitle, new Vector2(-160f, 190f), new Vector2(220f, 34f), 23, s_textColor, TextAlignmentOptions.Left);
            m_previewSummaryText = CreateText(m_previewPanel.transform, EventCalendarConstants.Ui.PreviewEmptyText, new Vector2(20f, 145f), new Vector2(460f, 42f), 15, s_mutedTextColor, TextAlignmentOptions.Left);
            m_retryFailedButton = CreateButton(m_previewPanel.transform, EventCalendarConstants.Ui.RetryFailedButtonName, EventCalendarConstants.Ui.RetryFailedButtonText, new Vector2(175f, 190f), new Vector2(150f, 38f), s_elevatedCardColor);
            m_retryFailedButton.interactable = false;
            CreatePreviewScrollArea(m_previewPanel.transform);

            m_statusText = CreateText(shell.transform, EventCalendarConstants.Status.Ready, new Vector2(0f, -330f), new Vector2(1060f, 38f), 16, s_mutedTextColor, TextAlignmentOptions.Center);
            m_musicStyleOptionsPanel.transform.SetAsLastSibling();
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
            return CreateCard(parent, position, size, s_cardColor, EventCalendarConstants.Ui.CardName);
        }

        private GameObject CreateCard(Transform parent, Vector2 position, Vector2 size, Color color)
        {
            return CreateCard(parent, position, size, color, EventCalendarConstants.Ui.CardName);
        }

        private GameObject CreateCard(Transform parent, Vector2 position, Vector2 size, Color color, string objectName)
        {
            GameObject card = new GameObject(objectName, typeof(RectTransform));
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

        private void ConfigureCanvasScaler(CanvasScaler canvasScaler)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        private TMP_InputField CreateInput(Transform parent, string label, string placeholderText, Vector2 position)
        {
            return CreateInput(parent, label, placeholderText, position, new Vector2(280f, 42f));
        }

        private TMP_InputField CreateInput(Transform parent, string label, string placeholderText, Vector2 position, Vector2 size)
        {
            CreateText(parent, label, position + new Vector2(-92f, 34f), new Vector2(240f, 26f), 15, s_mutedTextColor, TextAlignmentOptions.Left);

            GameObject inputObject = new GameObject(label, typeof(RectTransform));
            inputObject.transform.SetParent(parent, false);
            AddImage(inputObject, s_inputColor);

            TMP_InputField inputField = inputObject.AddComponent<TMP_InputField>();
            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchoredPosition = position;
            inputRect.sizeDelta = size;

            TextMeshProUGUI text = CreateText(inputObject.transform, string.Empty, Vector2.zero, new Vector2(size.x - 32f, size.y - 10f), 17, Color.black, TextAlignmentOptions.MidlineLeft);
            text.gameObject.name = EventCalendarConstants.Ui.InputTextObjectName;
            inputField.textComponent = text;

            TextMeshProUGUI placeholder = CreateText(inputObject.transform, placeholderText, Vector2.zero, new Vector2(size.x - 32f, size.y - 10f), 17, new Color(0.43f, 0.45f, 0.54f, 1f), TextAlignmentOptions.MidlineLeft);
            placeholder.gameObject.name = EventCalendarConstants.Ui.PlaceholderObjectName;
            inputField.placeholder = placeholder;

            return inputField;
        }

        private void CreateMusicStyleDropdown(Transform parent, Vector2 position, Vector2 size)
        {
            CreateText(parent, EventCalendarConstants.Ui.StyleLabel, position + new Vector2(-92f, 34f), new Vector2(240f, 26f), 15, s_mutedTextColor, TextAlignmentOptions.Left);

            m_musicStyleDropdownButton = CreateButton(parent, EventCalendarConstants.Ui.MusicStyleDropdownButtonName, string.Empty, position, size, s_inputColor);
            m_musicStyleDropdownLabel = CreateText(m_musicStyleDropdownButton.transform, EventCalendarConstants.Ui.StylePlaceholder, Vector2.zero, new Vector2(size.x - 38f, size.y - 10f), 17, Color.black, TextAlignmentOptions.MidlineLeft);

            m_musicStyleOptionsPanel = CreateCard(parent, position + new Vector2(0f, -158f), new Vector2(size.x, 250f), s_elevatedCardColor, EventCalendarConstants.Ui.MusicStyleOptionsPanelName);
            m_musicStyleOptionsPanel.SetActive(false);

            for (int index = 0; index < EventCalendarConstants.MusicStyles.Options.Length; index++)
            {
                string musicStyle = EventCalendarConstants.MusicStyles.Options[index];
                Toggle toggle = CreateMusicStyleToggle(m_musicStyleOptionsPanel.transform, musicStyle, new Vector2(0f, 98f - index * 30f), size.x - 34f);
                m_musicStyleToggles.Add(toggle);
            }
        }

        private Toggle CreateMusicStyleToggle(Transform parent, string label, Vector2 position, float width)
        {
            GameObject toggleObject = new GameObject(string.Format(EventCalendarConstants.Ui.MusicStyleToggleNameFormat, label), typeof(RectTransform));
            toggleObject.transform.SetParent(parent, false);

            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.anchoredPosition = position;
            toggleRect.sizeDelta = new Vector2(width, 26f);

            Toggle toggle = toggleObject.AddComponent<Toggle>();

            GameObject background = CreateCard(toggleObject.transform, new Vector2(-width / 2f + 16f, 0f), new Vector2(18f, 18f), s_toggleOffColor, EventCalendarConstants.Ui.CardName);
            GameObject checkmark = CreateCard(background.transform, Vector2.zero, new Vector2(10f, 10f), s_secondaryColor, EventCalendarConstants.Ui.CheckmarkObjectName);
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.targetGraphic = background.GetComponent<Image>();

            CreateText(toggleObject.transform, label, new Vector2(18f, 0f), new Vector2(width - 42f, 24f), 16, s_textColor, TextAlignmentOptions.MidlineLeft);
            return toggle;
        }

        private void CreatePreviewScrollArea(Transform parent)
        {
            GameObject viewport = CreateCard(parent, new Vector2(0f, -54f), new Vector2(500f, 300f), s_panelColor, EventCalendarConstants.Ui.PreviewViewportName);
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            m_previewContent = new GameObject(EventCalendarConstants.Ui.PreviewContentName, typeof(RectTransform));
            m_previewContent.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = m_previewContent.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 300f);

            ScrollRect scrollRect = m_previewPanel.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
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

        private void SetButtonLabel(Button button, string labelValue)
        {
            if (button == null)
            {
                return;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = labelValue;
            }
        }

        private void ConfigureDefaultMusicStyleSelection()
        {
            if (m_musicStyleToggles.Count == 0)
            {
                return;
            }

            m_musicStyleToggles[0].isOn = true;
            UpdateMusicStyleDropdownLabel();
        }

        private void ToggleMusicStyleOptions()
        {
            if (m_musicStyleOptionsPanel != null)
            {
                m_musicStyleOptionsPanel.SetActive(!m_musicStyleOptionsPanel.activeSelf);
            }
        }

        private void HandleMusicStyleSelectionChanged(bool _)
        {
            if (m_musicStyleToggles.Count == 0)
            {
                return;
            }

            bool hasSpecificSelection = false;
            for (int index = 1; index < m_musicStyleToggles.Count; index++)
            {
                hasSpecificSelection |= m_musicStyleToggles[index].isOn;
            }

            m_musicStyleToggles[0].SetIsOnWithoutNotify(!hasSpecificSelection);
            UpdateMusicStyleDropdownLabel();
            ClearPreviewIfSearchCriteriaChanged();
        }

        private void HandleSearchCriteriaChanged(string _)
        {
            ClearPreviewIfSearchCriteriaChanged();
        }

        private void ClearPreviewIfSearchCriteriaChanged()
        {
            if (!m_isImporting && m_hasPreviewResults)
            {
                ClearPreviewItems();
                SetStatus(EventCalendarConstants.Status.Ready);
            }
        }

        private string GetSelectedMusicStyleFilter()
        {
            List<string> selectedStyles = GetSelectedMusicStyles();
            return selectedStyles.Count == 0
                ? EventCalendarConstants.MusicStyles.All
                : string.Join(EventCalendarConstants.Search.MultipleStylesSeparator, selectedStyles);
        }

        private List<string> GetSelectedMusicStyles()
        {
            List<string> selectedStyles = new List<string>();

            for (int index = 1; index < m_musicStyleToggles.Count && index < EventCalendarConstants.MusicStyles.Options.Length; index++)
            {
                if (m_musicStyleToggles[index].isOn)
                {
                    selectedStyles.Add(EventCalendarConstants.MusicStyles.Options[index]);
                }
            }

            return selectedStyles;
        }

        private void UpdateMusicStyleDropdownLabel()
        {
            if (m_musicStyleDropdownLabel != null)
            {
                m_musicStyleDropdownLabel.text = GetSelectedMusicStyleFilter();
            }
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
