/*
                             <\              _
                               \\          _/{
  onyx                  _       \\       _-   -_
  ChapterSelect       /{        / `\   _-  -  - -_
  v^v^              _~  =      ( @  \ -    -   -  -_
  ready?          _- -   ~-_   \( \  \      -    -  -_
                _~  -   -   ~_ | 1 \  \      _-~-_ -  -_
              _-   -    -     ~  |V: \ \  _-~     ~-_-  -_
           _-~   -    -       /  | :  \ \            ~-_- -_
        _-~    -   _.._      {   | : _-``               ~- _-_
     _-~   -__..--~    ~-_  {   : \:}
   =~__.--~~              ~-_\  :  /
                              \ : /__              
                             //`Y'--\\      =       
                            <+       \\             
                             \\      WWW
                             MMM
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using ChapterSelect.Code;
using Koop;
using UnityEngine.Localization.Settings;

namespace ChapterSelect
{
    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
        public static object CallPrivate(this object o, string methodName, params object[] args)
        {
            var mi = o.GetType ().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Instance );
            return mi != null ? mi.Invoke (o, args) : null;
        }
    }

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource LOG;
        private static Harmony _harmony;
        public static AssetBundle Assets;
        public static readonly Dictionary<string, Texture2D> Textures = new();

        /**
         * Plugin entry. This has 3 responsibilities:
         * - Set up the managers
         * - Insert localization info
         * - Apply patches
         *
         * Do nothing else here. Put it in a manager if it's important.
         */
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            LOG = BepInEx.Logging.Logger.CreateLogSource("ChapterSelect");
            BepInEx.Logging.Logger.Sources.Add(LOG);
            
            // inject managers; kindly place them alongside the game's managers
            var managerMaster = GameObject.Find("MANAGER_MASTER");
            var chaptersMaster = new GameObject("MOD_ChapterSelect");
            chaptersMaster.transform.SetParent(managerMaster.transform);
            
            var gameAssetLoader = new GameObject("CS_GameAssetLoader");
            DontDestroyOnLoad(gameAssetLoader);
            gameAssetLoader.transform.SetParent(chaptersMaster.transform);
            gameAssetLoader.AddComponent<Mgr_CS_GameAssetLoader>().enabled = true;
            
            var chapterSelect = new GameObject("ChapterSelectManager");
            DontDestroyOnLoad(chapterSelect);
            chapterSelect.transform.SetParent(chaptersMaster.transform);
            chapterSelect.AddComponent<Mgr_ChapterSelect>().enabled = true;
            
            var uiInjection = new GameObject("CS_UIInjection");
            DontDestroyOnLoad(uiInjection);
            uiInjection.transform.SetParent(chaptersMaster.transform);
            uiInjection.AddComponent<Mgr_CS_UIInjection>().enabled = true;
            
            var saveManagement = new GameObject("CS_SaveManagement");
            DontDestroyOnLoad(saveManagement);
            saveManagement.transform.SetParent(chaptersMaster.transform);
            saveManagement.AddComponent<Mgr_CS_SaveManagement>().enabled = true;
            
            var table = LocalizationSettings.StringDatabase.GetTable("MISC", LocalizationSettings.AvailableLocales.GetLocale("en"));
            foreach (var element in Localization.GetEnglishLocalizations())
            {
                table.AddEntry(element.Key, element.Value);
            }

            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll();
            Assets = OnyxUtils.LoadEmbeddedAssetBundle("ChapterSelect.Assets.chaptermod.assets");
            
            foreach (var scene in SelectableScene.AllScenes)
            {
                Textures[scene.SceneName] = Assets.LoadAsset<Texture2D>("CH_I_" + scene.SceneName);
            }
        }
    }

    /**
     * Plugin Required Hooks. They are all here.
     *
     * Some guidelines:
     * - Add as few hooks as possible. They damage readability as they lack surrounding file context.
     * - Forward to managers if a function is getting too long. Keep it brief.
     */
    
    [HarmonyPatch(typeof(Mgr_LevelFlow))]
    [HarmonyPatch("ReadyToRevealNewScene")]
    public class RevealNewScenePatch
    {
        static void Postfix()
        {
            Mgr_ChapterSelect.Instance.CallPrivate("_SceneLoadedInternal");
        }
    }

    [HarmonyPatch(typeof(Mgr_LevelFlow))]
    [HarmonyPatch("OnSceneLoaded")]
    public class OnSceneLoadedPatch
    {
        static void Postfix(Mgr_LevelFlow __instance, UnityEngine.SceneManagement.Scene scene)
        {
            Mgr_ChapterSelect.Instance.CurrentScene = scene.name;
        }
    }
    
    // Targeting specifically the instance method.
    [HarmonyPatch(typeof(Mgr_Achievements))]
    [HarmonyPatch("AwardAchievement")]
    [HarmonyPatch(new Type[] { typeof(SO_Achievement) })]
    public class AwardAchievementPatch
    {
        static bool Prefix(SO_Achievement ach)
        {
            return !Mgr_ChapterSelect.Instance.IsInThrowback;
        }
    }
    
    
    [HarmonyPatch(typeof(TitleScreen))]
    [HarmonyPatch("Awake")]
    public class TitleScreenAwakePatch
    {
        static void Prefix(TitleScreen __instance)
        {
            if (Mgr_ChapterSelect.Instance.IsInThrowback)
                Mgr_ChapterSelect.Instance.EndThrowback(); // restores save state
        }
    }
    
    /**
     * Whenever we're in a throwback, we want to jump back to the title scene when the scene finishes
     * Implementing it like this is the preferred way as it won't break in-scene minigames :>
     */
    [HarmonyPatch(typeof(Helper_Ink))]
    [HarmonyPatch("ContainsSpecialCall")]
    public class ContainsSpecialCallPatch
    {
        static bool Prefix(string speak, bool doAction, ref bool __result)
        {
            if (!Mgr_ChapterSelect.Instance.IsInThrowback)
                return true;
            if (speak.Contains("NEXT:"))
            {
                if (doAction)
                {
                    Mgr_LevelFlow.Instance.nextSceneFromInk = "Title";
                    InkMaster.ActiveInstance.Continue();
                }
                __result = true;
                return false;
            }
            return true;    
        }
    }
    
    /**
     * Patch to disable saves whenever we're in a throwback.
     */
    [HarmonyPatch(typeof(Mgr_Saves))]
    [HarmonyPatch("SaveGameplay")]
    public class SaveGameplayPatch
    {
        private static IEnumerator PostSavedNotif()
        {
            // Our prefix might prevent this callback from being sent
            // If anything (?) uses this, it may block, so it's probably a good idea to send it as expected.
            Notify gameDataSavedToDisk = Mgr_Saves.GameDataSavedToDisk;
            if (gameDataSavedToDisk != null)
            {
                gameDataSavedToDisk();
            }

            yield break;
        }

        static bool Prefix()
        {
            if (Mgr_CS_SaveManagement.Instance == null)
                return true;
            if (Mgr_CS_SaveManagement.Instance.SavesEnabled)
                return true;
            Mgr_ChapterSelect.Instance.StartCoroutine(PostSavedNotif());
            return false;
        }
    }
}