using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace ChapterSelect.Code;

/**
 * Primary Component for the Chapters Menu
 *
 * This also contains code for building the Chapters Menu UI.
 */
public class ChaptersMenu : TabbedMenu // => Interactable |
{
    public override void OnEnable()
    {
        base.OnEnable();

        // StartCoroutine(SelectableScene.TestAndLogSceneVisibility());
        var tabContainer = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1);
        foreach (var child in tabContainer.GetComponentsInChildren<BasicMenuTab>())
        {
            child.text.enableWordWrapping = false;
            child.text.enableAutoSizing = true;
        }
    }

    /**
     * Build the UI for the Chapters Menu
     *
     * This would've ideally been a prefab, but I am heavily avoiding dumping and reshipping _any_ of the game's assets,
     *  so we manually compose it here.
     */
    public void BuildUI()
    {
        // background translucent black layer
        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(transform);
        var backgroundTransform = background.GetComponent<RectTransform>();
        backgroundTransform.anchorMin = new Vector2(0, 0);
        backgroundTransform.anchorMax = new Vector2(1, 1);
        backgroundTransform.offsetMin = new Vector2(0, 0);
        backgroundTransform.offsetMax = new Vector2(0, 0);
        backgroundTransform.localScale = new Vector3(1, 1, 1);
        background.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

        var mainRegion = new GameObject("MainRegion", typeof(RectTransform));
        var mainRegionRect = mainRegion.GetComponent<RectTransform>();
        mainRegionRect.SetParent(transform, false);
        mainRegionRect.anchorMin = new Vector2(0, 0);
        mainRegionRect.anchorMax = new Vector2(1, 1);
        mainRegionRect.pivot = new Vector2(0.5f, 0.5f);
        mainRegionRect.offsetMin = new Vector2(0, 0);
        mainRegionRect.offsetMax = new Vector2(0, 0);
        mainRegionRect.localScale = new Vector3(1, 1, 1);

        var titleAndTabs = new GameObject("TitleAndTabs", typeof(RectTransform), 
            typeof(HorizontalLayoutGroup), typeof(ContentSizeFitterIgnoreIfLayoutChild));
        var titleAndTabsHorizontalLayoutGroup = titleAndTabs.GetComponent<HorizontalLayoutGroup>();
        titleAndTabsHorizontalLayoutGroup.childForceExpandWidth = false;
        titleAndTabsHorizontalLayoutGroup.spacing = 50;
        var titleAndTabsRect = titleAndTabs.GetComponent<RectTransform>();
        titleAndTabsRect.SetParent(mainRegion.transform, false);
        titleAndTabsRect.localScale = new Vector3(1, 1, 1);
        titleAndTabsRect.anchorMin = new Vector2(0, 1);
        titleAndTabsRect.anchorMax = new Vector2(1, 1);
        titleAndTabsRect.pivot = new Vector2(0.5f, 1);
        titleAndTabsRect.sizeDelta = new Vector2(0, 400);
        titleAndTabsRect.offsetMin = new Vector2(160, -360);
        titleAndTabsRect.offsetMax = new Vector2(-160, 40);

        var titleText = new GameObject("Title", typeof(TextMeshProUGUI));
        var textMeshPro = titleText.GetComponent<TextMeshProUGUI>();
        textMeshPro.text = "Chapters";
        textMeshPro.fontSize = 86; // Set font size as needed
        textMeshPro.alignment = TextAlignmentOptions.Left; // Align text to the left
        textMeshPro.enableWordWrapping = false;
        textMeshPro.enableAutoSizing = true;
        
        var titleRect = titleText.GetComponent<RectTransform>();
        titleRect.SetParent(titleAndTabs.transform);
        titleRect.anchorMin = new Vector2(0, 0.5f);
        titleRect.anchorMax = new Vector2(0, 0.5f);
        titleRect.pivot = new Vector2(0, 0.5f);
        titleRect.offsetMax = new Vector2(1765, 100);
        titleRect.offsetMin = new Vector2(165, -100);
        titleRect.localScale = new Vector3(1, 1, 1);

        var titleSpacer = new GameObject("TitleSpacer");
        var titleSpacerRect = titleSpacer.AddComponent<RectTransform>();
        titleSpacerRect.SetParent(titleAndTabs.transform);
        titleSpacer.AddComponent<LayoutElement>().flexibleWidth = 1;

        var tabContainer = new GameObject("TabContainer", typeof(RectTransform), 
            typeof(HorizontalLayoutGroup), typeof(ContentSizeFitterIgnoreIfLayoutChild));
        var tabContainerRect = tabContainer.GetComponent<RectTransform>();
        tabContainerRect.SetParent(titleAndTabs.transform);
        tabContainerRect.anchorMin = new Vector2(0, 0);
        tabContainerRect.anchorMax = new Vector2(1, 0);
        tabContainerRect.pivot = new Vector2(0, 0);
        tabContainerRect.offsetMin = new Vector2(165, -100);
        tabContainerRect.offsetMax = new Vector2(1765 + 165, 100);
        tabContainerRect.localScale = new Vector3(1, 1, 1);

        tabContainer.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
        tabContainer.GetComponent<ContentSizeFitterIgnoreIfLayoutChild>().horizontalFit 
            = ContentSizeFitterIgnoreIfLayoutChild.FitMode.PreferredSize;

        var tabLeftClone = Instantiate(Mgr_CS_GameAssetLoader.Instance.tabLeft, tabContainer.transform);
        tabLeftClone.name = "TabLeftClone";
        tabLeftClone.transform.localScale = new Vector3(1, 1, 1);
        // var dynGlyph = tabLeftClone.GetComponentInChildren<DynamicInputGlyph>();
        
        var menuTabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup),
                    typeof(ContentSizeFitterIgnoreIfLayoutChild));
        menuTabs.GetComponent<RectTransform>().localScale = new Vector3(0.85f/2, 0.85f/2, 0.85f/2);
        menuTabs.GetComponent<HorizontalLayoutGroup>().spacing = 80;
        menuTabs.transform.SetParent(tabContainer.transform);

        var tabRightClone = Instantiate(Mgr_CS_GameAssetLoader.Instance.tabRight, tabContainer.transform);
        tabRightClone.name = "TabRightClone";
        tabRightClone.transform.localScale = new Vector3(1, 1, 1);

        var main = new GameObject("ChapterContentRegion", typeof(RectTransform));
        var chapterContentRegionRect = main.GetComponent<RectTransform>();
        chapterContentRegionRect.offsetMin = new Vector2(0, -200);
        chapterContentRegionRect.offsetMax = new Vector2(0, -200);
        // pin along bottom edge
        chapterContentRegionRect.anchorMin = new Vector2(0, 0);
        chapterContentRegionRect.anchorMax = new Vector2(1, 1);
        chapterContentRegionRect.pivot = new Vector2(0.5f, 0.5f);
        chapterContentRegionRect.SetParent(mainRegion.transform, false);

        var chapterPages = new MenuPage[8];
        for (var i = 1; i <= 8; i++)
        {
            var chapterPage = new GameObject("Chapter " + i, typeof(RectTransform), 
                typeof(SelectableGroupController), typeof(ChaptersPage));
            var chapterPageRect = chapterPage.GetComponent<RectTransform>();
            chapterPageRect.SetParent(main.transform, false);
            chapterPageRect.localScale = new Vector3(1, 1, 1);
            chapterPageRect.anchorMax = new Vector2(1, 1);
            chapterPageRect.anchorMin = new Vector2(0, 0);
            chapterPageRect.pivot = new Vector2(0.5f, 0.5f);
            chapterPageRect.offsetMin = new Vector2(0, 0);
            chapterPageRect.offsetMax = new Vector2(0, 0);
            
            chapterPage.GetComponent<SelectableGroupController>().enabled = true;
            
            var page = chapterPage.GetComponent<ChaptersPage>();
            page.columnCount = 4;
            page.tabName = "Ep. " + i;
            page.chapter = i;
            page.enabledOnThisPlatform = new PlatformFlags();
            page.enabledOnThisPlatform.platformFlags |= Platform.PC;
            page.enabledOnThisPlatform.platformFlags |= Platform.Steamdeck;
            page.enabled = true;

            chapterPages[i - 1] = chapterPage.GetComponent<ChaptersPage>();
        }

        var horizPromptButtonSet = new GameObject("HorizontalPromptButtonSet");
        var horizPromptButtonSetRect = horizPromptButtonSet.AddComponent<RectTransform>();
        horizPromptButtonSetRect.SetParent(mainRegion.transform, false);
        horizPromptButtonSetRect.localScale = new Vector3(1, 1, 1);
        horizPromptButtonSetRect.anchorMin = new Vector2(1, 0);
        horizPromptButtonSetRect.anchorMax = new Vector2(1, 1);
        horizPromptButtonSetRect.pivot = new Vector2(1f, 1f);
        horizPromptButtonSetRect.sizeDelta = new Vector2(0, 0);
        horizPromptButtonSetRect.offsetMin = new Vector2(-1000, 150);
        horizPromptButtonSetRect.offsetMax = new Vector2(-160, 150);
        var horizPromptButtonSetHorizontalLayoutGroup = horizPromptButtonSet.AddComponent<HorizontalLayoutGroup>();
        horizPromptButtonSetHorizontalLayoutGroup.childControlWidth = true;
        horizPromptButtonSetHorizontalLayoutGroup.childControlHeight = true;
        horizPromptButtonSetHorizontalLayoutGroup.childForceExpandWidth = false;
        horizPromptButtonSetHorizontalLayoutGroup.childForceExpandHeight = false;
        horizPromptButtonSetHorizontalLayoutGroup.spacing = 78;
        horizPromptButtonSetHorizontalLayoutGroup.reverseArrangement = true;
        horizPromptButtonSetHorizontalLayoutGroup.childAlignment = TextAnchor.LowerRight;
        
        var backButton = Instantiate(Mgr_CS_GameAssetLoader.Instance.tabLeft, horizPromptButtonSet.transform);  
        backButton.name = "BackButton";
        backButton.transform.localScale = new Vector3(1, 1, 1);
        backButton.GetComponent<RewiredActionButtonDownListener>().RewiredAction = 76;
        backButton.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 100);
        var label = new GameObject("BackButtonLabel", typeof(TextMeshProUGUI));
        label.transform.SetParent(backButton.transform);
        label.GetComponent<TextMeshProUGUI>().text = "Back";
        label.GetComponent<TextMeshProUGUI>().font = Mgr_CS_GameAssetLoader.Instance.standardFontAsset;
        
        var selButton = Instantiate(Mgr_CS_GameAssetLoader.Instance.tabLeft, horizPromptButtonSet.transform);
        selButton.name = "SelectButton";
        selButton.transform.localScale = new Vector3(1, 1, 1);
        selButton.GetComponent<RewiredActionButtonDownListener>().RewiredAction = 0;
        selButton.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 100);
        var selButtonLabel = new GameObject("SelectButtonLabel", typeof(TextMeshProUGUI));
        selButtonLabel.transform.SetParent(selButton.transform);
        selButtonLabel.GetComponent<TextMeshProUGUI>().text = "Select";
        selButtonLabel.GetComponent<TextMeshProUGUI>().font = Mgr_CS_GameAssetLoader.Instance.standardFontAsset;
        
        pages = chapterPages;

        MainCanvasGroup = GetComponent<CanvasGroup>();
        TabContainer = menuTabs.transform;
        TabRow = titleAndTabsRect;
        tabNameField = null;
        tabController = GetComponent<SelectableGroupController>();
        controlsLayout = null;
        this.controlPrefab = Mgr_CS_GameAssetLoader.Instance.controlPrefab;
        tabPrefab = Mgr_CS_GameAssetLoader.Instance.tabPrefab;
        this.ruleset_whileOpen = "MenuUI";

        input_back = 76;
        input_nextTab = 52;
        input_previousTab = 51;
    }
}