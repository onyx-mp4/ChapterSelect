using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChapterSelect.Code;

public class Mgr_CS_UIInjection : MonoBehaviour
{
    private static Mgr_CS_UIInjection _instance;
    public static Mgr_CS_UIInjection Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Mgr_CS_UIInjection>();
            }

            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private IEnumerator Start()
    {
        if (Mgr_ChapterSelect.Instance == null)
            yield return null;
        if (Mgr_CS_GameAssetLoader.Instance == null)
            yield return null;
        
        Mgr_ChapterSelect.Instance.SceneReadyCallbacks.Add(TitleScreenReady);
    }

    public void TitleScreenReady()
    {
        Plugin.LOG.LogInfo($"Title screen ready called with {Mgr_ChapterSelect.Instance.CurrentScene}");
        if (Mgr_ChapterSelect.Instance.CurrentScene != "Title")
            return;

        Mgr_CS_GameAssetLoader.Instance.RunFullObjectSweep();
        
        Plugin.LOG.LogInfo("Game assets loaded");

        try
        {
            var button = InjectChaptersButton();
            if (button == null)
            {
                Plugin.LOG.LogError("Failed to inject button! This is bad! Did the game get updated?");
                return;
            }
            InjectChaptersMenu(button);
        }
        catch (Exception e)
        {
            Plugin.LOG.LogError($"Failed to inject chapters menu: {e}");
            Plugin.LOG.LogError(e.StackTrace);
        }

        
        Plugin.LOG.LogInfo("Injection Complete");
    }

    /**
     * Responsibilities:
     * 
     * 1. Manually build the UI for our button  
     * 2. Inject it into the main menu  
     * 3. Rewire the main menu buttons to accommodate our new button  
     * 4. Return the injected button
     */
    private UiSelectable4WayButton InjectChaptersButton()
    {
        var container = GameObject.Find("Screen1/ButtonContainer");
        if (container != null)
        {
            var horGroup = Instantiate(container.transform.GetChild(0), container.transform, true);
            foreach (Transform child in horGroup.transform)
            {
                Destroy(child.gameObject);
            }

            var settingsTransform = container.transform.GetChild(1);
            settingsTransform.SetParent(horGroup.transform);
            settingsTransform.localScale = new Vector3(1, 1, 1);

            var button = Instantiate(Mgr_CS_GameAssetLoader.Instance.mainMenuButtonPrefab, horGroup);
            button.name = "Chapters";
            var textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = "Chapters";
            button.transform.localScale = new Vector3(1, 1, 1);

            button.GetComponentInChildren<StaticStringLocaliser>().BaseText = "Chapters";
            
            // force this, it looks better imo
            typeof(UiSelectable4WayButton).GetField("_textOverflowResponse",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            )?.SetValue(button.GetComponent<UiSelectable4WayButton>(), 0);

            var chaptersEventTrigger = button.GetComponent<EventTrigger>();
            chaptersEventTrigger.triggers.Clear();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((_) =>
            {
                Plugin.LOG.LogInfo("Chapters button clicked!");
            });
            chaptersEventTrigger.triggers.Add(entry);

            horGroup.transform.SetSiblingIndex(1);
            horGroup.transform.localScale = new Vector3(1, 1, 1);

            var newGameButton = GameObject.Find("New Game").GetComponent<UiSelectable4WayButton>();
            var continueButton = GameObject.Find("Continue").GetComponent<UiSelectable4WayButton>();

            var settingsButton = GameObject.Find("Settings").GetComponent<UiSelectable4WayButton>();
            var chaptersButton = GameObject.Find("Chapters").GetComponent<UiSelectable4WayButton>();
            var quitButton = GameObject.Find("Quit").GetComponent<UiSelectable4WayButton>();

            continueButton.downSelection = button.GetComponent<UiSelectable4WayButton>();
            continueButton.rightSelection = newGameButton;
            continueButton.leftSelection = newGameButton;
            continueButton.upSelection = quitButton;

            settingsButton.rightSelection = button.GetComponent<UiSelectable4WayButton>();
            settingsButton.leftSelection = button.GetComponent<UiSelectable4WayButton>();
            settingsButton.upSelection = newGameButton;

            chaptersButton.downSelection = quitButton;
            chaptersButton.leftSelection = settingsButton;
            chaptersButton.rightSelection = settingsButton;
            chaptersButton.upSelection = continueButton;

            return chaptersButton;
        }
        else
        {
            return null;
        }
    }

    /**
     * 1. Manually create our chapter menu prefab
     * 2. Give it a ChaptersMenu component, then have that build the _bulk_ of its UI.
     * 3. Inject into UI
     * 4. Wire the button up to call .Open() on this menu
     *
     * This deviates a bit from standard practice in terms of a Component managing its own UI so heavily,
     *     but with the tradeoff of being far far easier to mentally parse in a fully-script-based environment.
     *
     * <param name="chaptersButton">
     *   Button we're going to wire up to open the chapters menu
     * </param>
     */
    public void InjectChaptersMenu(UiSelectable4WayButton chaptersButton)
    {
        Plugin.LOG.LogInfo("Injecting chapters menu");

        var chaptersMenu = new GameObject("ChaptersMenu", typeof(RectTransform), typeof(CanvasGroup), 
            typeof(Canvas), typeof(GraphicRaycaster), typeof(SelectableGroupController));
        chaptersMenu.SetActive(false);
        chaptersMenu.transform.SetParent(GameObject.Find("Screen1").transform);
        var chaptersMenuRect = chaptersMenu.GetComponent<RectTransform>();
        chaptersMenuRect.anchorMin = new Vector2(0, 0);
        chaptersMenuRect.anchorMax = new Vector2(1, 1);
        chaptersMenuRect.offsetMin = new Vector2(0, 0);
        chaptersMenuRect.offsetMax = new Vector2(0, -0);
        chaptersMenu.transform.localScale = new Vector3(1, 1, 1);

        var menu = chaptersMenu.AddComponent<ChaptersMenu>();
        menu.enabled = true;
        menu.BuildUI();

        var buttonContainer = GameObject.Find("ButtonContainer");
        var logo = GameObject.Find("Logo");
        var chaptersEventTrigger = chaptersButton.gameObject.GetComponent<EventTrigger>();
        chaptersEventTrigger.triggers.Clear();
        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener((_) =>
        {
            buttonContainer.SetActive(false);
            logo.SetActive(false);
            menu.Open();
            menu.enabled = true;
        });
        menu.onClosed = all =>
        {
            if (all)
                return;
            buttonContainer.SetActive(true);
            logo.SetActive(true);
        };
        chaptersEventTrigger.triggers.Add(entry);
    }
}