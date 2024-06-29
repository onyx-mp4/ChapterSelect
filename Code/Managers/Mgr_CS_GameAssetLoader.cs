using TMPro;
using UnityEngine;

namespace ChapterSelect.Code;

/*
 * Singleton responsible for assets
 *
 * It is theoretically possible to reassemble all of these fabs from the game's assets.
 * One would do this either by dumping the game's assets and reassembling them, or by
 * manually rebuilding them by hand with scripting. I am heavily avoiding the former and the latter would take
 * years.
 *
 * So instead, we do a singular quick runthrough of game objects and grab the 'prefabs' that we need.
 */
public class Mgr_CS_GameAssetLoader : MonoBehaviour
{
    // Pulled from main menu, 'prefab' needed for button
    public GameObject mainMenuButtonPrefab;
    public GameObject tabPrefab;
    
    // Pulled from settings menu, these are some 'prefabs' we need for Chapters Menu
    public GameObject tabLeft;
    public GameObject tabRight;
    public GameObject controlPrefab;
    public GameObject settingsPageContentCanvasGroup;
    
    // Primary font asset. This is a licensed asset and this is about the only way I can properly utilize it. 
    public TMP_FontAsset standardFontAsset;
    
    private static Mgr_CS_GameAssetLoader _instance;
    public static Mgr_CS_GameAssetLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Mgr_CS_GameAssetLoader>();
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

    public void RunFullObjectSweep()
    {
        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in objects)
        {
            switch (obj.name)
            {
                case "TabLeft":
                    tabLeft = obj;
                    break;
                case "TabRight":
                    tabRight = obj;
                    break;
                case "SettingsMenu":
                    controlPrefab = obj.GetComponent<SettingsMenu>()
                        .GetFieldValue<GameObject>("controlPrefab");
                    tabPrefab = obj.GetComponent<SettingsMenu>()
                        .GetFieldValue<GameObject>("tabPrefab");
                    settingsPageContentCanvasGroup = obj.GetComponent<SettingsMenu>().pages[0].canvasGroup.gameObject;
                    break;
                case "ButtonContainer":
                    if (obj.transform.parent.name != "Screen1")
                        break;
                    mainMenuButtonPrefab = obj.transform.GetChild(1).gameObject;
                    standardFontAsset = mainMenuButtonPrefab.GetComponent<UiSelectable4WayButton>().label.font;
                    break;
            }
        }
    }
}