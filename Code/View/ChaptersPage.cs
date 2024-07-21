using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChapterSelect.Code;


public class ChaptersPage : TwoAxisElementListPage // => ElementListPage => MenuPage |
{
    private List<SelectableScene> _scenes;
    public int chapter = 1;

    protected override void Awake()
    {
        Plugin.LOG.LogInfo("Awake ChaptersPage");
        elementController = GetComponent<SelectableGroupController>();
        base.Awake();
    }

    public override void GetControlsToAdd(List<LabelledRewiredAction> actions)
    {
    }

    public override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(SelectFirstWhenReady());
    }

    private IEnumerator SelectFirstWhenReady()
    {
        if (!GetComponent<SelectableGroupController>().GetElement(0).isActiveAndEnabled)
            yield return null;

        GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1;
    }

    public override void InitializePage()
    {
        Plugin.LOG.LogInfo("Initializing ChaptersPage");
        try
        {
            base.InitializePage();
            var gsd = Mgr_Saves.saveData;
            var variables = OnyxUtils.ParseSaveDataString(gsd.globalVariables);
            _scenes = SelectableScene.ScenesInChapter(chapter).Where(scene => scene.SceneVisibility(gsd, variables) != SelectableScene.Visibility.Hidden).ToList();
            
            var contentCanvasGroup = Instantiate(Mgr_CS_GameAssetLoader.Instance.settingsPageContentCanvasGroup, transform);
            var viewport = contentCanvasGroup.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            var contentParent =  contentCanvasGroup.transform.GetChild(0).gameObject;
            var content = contentParent.transform.GetChild(0);
            var scrollbar = content.transform.GetChild(1);
            // killing the Items box. 
            Destroy(viewport.transform.GetChild(0).gameObject);
            var itemGrid = new GameObject("Items", typeof(RectTransform), typeof(GridLayoutGroup),
                typeof(ContentSizeFitterIgnoreIfLayoutChild));
            itemGrid.transform.SetParent(viewport.transform);
            itemGrid.transform.localScale = new Vector3(1, 1, 1);
            itemGrid.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            itemGrid.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            itemGrid.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            itemGrid.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            itemGrid.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            content.GetComponent<ScrollRect>().content = itemGrid.GetComponent<RectTransform>();

            var itemGridGrid = itemGrid.GetComponent<GridLayoutGroup>();
            itemGridGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            itemGridGrid.constraintCount = 4;
            itemGridGrid.padding.bottom += 500;
            itemGridGrid.cellSize = new Vector2(768, 432);
            itemGridGrid.spacing = new Vector2(150, 75);
            itemGridGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            
            var itemGridContentSizeFitter = itemGrid.AddComponent<ContentSizeFitterIgnoreIfLayoutChild>();
            itemGridContentSizeFitter.horizontalFit = ContentSizeFitterIgnoreIfLayoutChild.FitMode.PreferredSize;
            itemGridContentSizeFitter.verticalFit = ContentSizeFitterIgnoreIfLayoutChild.FitMode.PreferredSize;

            var contentParentRect = contentParent.GetComponent<RectTransform>();
            contentParentRect.anchorMin = new Vector2(0, 0);
            contentParentRect.anchorMax = new Vector2(1, 1);
            contentParentRect.pivot = new Vector2(0.5f, 0.5f);
            contentParentRect.offsetMin = new Vector2(0, 0);
            contentParentRect.offsetMax = new Vector2(0, -100);
            contentParentRect.localScale = new Vector3(1, 1, 1); 
            var csf = contentParent.AddComponent<ContentSizeFitterIgnoreIfLayoutChild>();
            csf.horizontalFit = ContentSizeFitterIgnoreIfLayoutChild.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitterIgnoreIfLayoutChild.FitMode.Unconstrained;

            var scrollEvent = new Scrollbar.ScrollEvent();
            scrollEvent.AddListener((value) =>
            {
                content.GetComponent<ScrollRect>().verticalNormalizedPosition = value;
            });
            scrollbar.GetComponent<Scrollbar>().onValueChanged = scrollEvent;
            OurScrollRect = content.GetComponent<ScrollRect>();
            
            canvasGroup = contentCanvasGroup.GetComponent<CanvasGroup>();
            var field = GetType().BaseType?.BaseType?.GetField("scrollRect", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(this, content.GetComponent<ScrollRect>());
            }
            
            var sceneElementPrefab = Instantiate(Plugin.Assets.LoadAsset<GameObject>("SceneCardPrefab"));
            sceneElementPrefab.GetComponentInChildren<TextMeshProUGUI>().font =
                Mgr_CS_GameAssetLoader.Instance.standardFontAsset;
            var element = sceneElementPrefab.AddComponent<SceneElement>();
            element.parentPage = this;
            element.onSelected = (SelectableElement.SelectedCallback)Delegate.CreateDelegate(typeof(SelectableElement.SelectedCallback), GetComponent<SelectableGroupController>(), "OnSelected");
            sceneElementPrefab.AddComponent<StaticStringLocaliser>();
            
            var guitarAsset = Plugin.Assets.LoadAsset<Texture2D>("guitar");
            var wormAsset = Plugin.Assets.LoadAsset<Texture2D>("ilovethiswormsofreakingmuch");
            
            for (var i = 0; i < _scenes.Count; i++)
            {
                var sceneElement = Instantiate(sceneElementPrefab, itemGrid.transform);
                // not a fan of storing "name that is critical" on the object name but i'm working with what i've got
                sceneElement.name = _scenes[i].SceneName;
                sceneElement.GetComponent<SceneElement>().onSelected = element.onSelected;
                var vis = _scenes[i].SceneVisibility(gsd, variables);
                sceneElement.GetComponentInChildren<TextMeshProUGUI>().text = "<b> " + (i+1) + "</b>   " +
                                                                              (vis == SelectableScene.Visibility.Visible ? _scenes[i].DisplayName : "???");
                switch (_scenes[i].Minigame)
                {
                    case SelectableScene.MinigameType.None:
                        sceneElement.transform.GetChild(3).gameObject.SetActive(false);
                        break;
                    case SelectableScene.MinigameType.Music:
                        sceneElement.transform.GetChild(3).GetComponent<RawImage>().texture = guitarAsset;
                        break;
                    case SelectableScene.MinigameType.Worm:
                        sceneElement.transform.GetChild(3).GetComponent<RawImage>().texture = wormAsset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (vis == SelectableScene.Visibility.Visible)
                {
                    sceneElement.transform.GetChild(0).GetComponent<RawImage>().texture =
                        Plugin.Textures[_scenes[i].SceneName];
                    sceneElement.GetComponent<StaticStringLocaliser>().BaseText = _scenes[i].DisplayName;
                }
                else
                {
                    sceneElement.transform.GetChild(0).GetComponent<RawImage>().texture = Plugin.Textures["locked"];;
                    sceneElement.GetComponent<StaticStringLocaliser>().BaseText = "???";
                }
                
                GetComponent<SelectableGroupController>().elements.Add(sceneElement.GetComponent<SelectableElement>());
                sceneElement.GetComponent<StaticStringLocaliser>().textObj =
                    sceneElement.GetComponent<TextMeshProUGUI>();
                sceneElement.GetComponent<StaticStringLocaliser>().enabled = true;
                sceneElement.GetComponent<SceneElement>().index = i;
                instantlySwapInAndOut.Add(sceneElement);
            }
            
            GetComponent<SelectableGroupController>().SelectElementNoNotify(0);
            GetComponent<SelectableGroupController>().SelectElement(0);
        }
        catch (Exception e)
        {
            Plugin.LOG.LogError("Failed to initialize ChaptersPage: " + e);
            Plugin.LOG.LogError(e.StackTrace);
        }
    }
}