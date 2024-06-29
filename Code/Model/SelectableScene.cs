using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ChapterSelect.Code;

public class SelectableScene
{
    public enum Visibility
    {
        Visible,
        Hidden,
        Locked
    }

    public enum MinigameType
    {
        None,
        Music,
        Worm
    }
    
    public readonly string SceneName;
    public readonly string DisplayName;
    public string Description;
    public readonly MinigameType Minigame;
    private readonly int _chapter;
    private readonly Visibility? _overridenVisibility;
    
    public static readonly List<SelectableScene> AllScenes = _GenerateAllScenes();
    private static readonly Dictionary<string, SelectableScene> SceneNameToSceneMap = _GenerateSceneNameToSceneMap();

    private SelectableScene(string sceneName, string displayName, string description, int chapter,
                            Visibility? overridenVisibility = null, MinigameType minigameType = MinigameType.None)
    {
        SceneName = sceneName;
        DisplayName = displayName;
        Description = description;
        _chapter = chapter;
        Minigame = minigameType;
        _overridenVisibility = overridenVisibility;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public Visibility SceneVisibility(GameplaySaveData data, Dictionary<string, object> saveVariables)  // hot method
    {
        if (_overridenVisibility != null)
            return (Visibility)_overridenVisibility;
        if (data?.sceneName == null)
            return SceneName == "E1_00_Beach" ? Visibility.Visible : Visibility.Locked;
        var savedSceneName = data.sceneName;
        
        if (savedSceneName == SceneName)
            return Visibility.Visible;

        if (!SceneNameToSceneMap.TryGetValue(savedSceneName, out var savedScene))
        {
            Plugin.LOG.LogWarning("Scene " + savedSceneName + " not found in scene map");
            return Visibility.Hidden;
        }

        if (_chapter > savedScene._chapter)
            return Visibility.Locked;
        
        // 😬 so there is totally a .previousSceneHistory, but in my save this field is completely empty. 
        // I have no clue if that's universal, so i'm just doing this the hard way. redundancy good :D

        // InkMaster.ActiveInstance.InkStory.variablesState.GetVariableWithName
        
        // Scenealts for calderaDress
        // there is an entire system for alt scenes used exclusively for the caldera dress variable ?
        // What Happened Here
        if (Mgr_LevelFlow.Instance.CallPrivate("getSceneAlternate", SceneName) is Mgr_LevelFlow.SceneAlternate alt)
        {
            if (saveVariables.ContainsKey(alt.boolName) 
                && saveVariables[alt.boolName] is bool
                && (bool)saveVariables[alt.boolName] == alt.boolConditionForAltScene) 
            {
                return Visibility.Visible;
            }
            return Visibility.Hidden;
        }
        // alt scenes are when calderaDress is false => non-alt scenes when you did date naomi => this is the canon path => thank you for coming to my ted talk

        if (SceneName == "E1_04A_ReedWedge")
        {
            if (saveVariables.ContainsKey("reedWedge") && saveVariables["reedWedge"] is bool && (bool)saveVariables["reedWedge"])
                return Visibility.Visible;
            return Visibility.Hidden;
        }
        if (SceneName == "E1_04B_TrishWedge")
        {
            if (saveVariables.ContainsKey("trishWedge") && saveVariables["trishWedge"] is bool && (bool)saveVariables["trishWedge"])
                return Visibility.Visible;
            return Visibility.Hidden;
        }
        if (SceneName == "E2_06B_NaomiWedge")
        {
            if (saveVariables.ContainsKey("naomiWedge") && saveVariables["naomiWedge"] is bool && (bool)saveVariables["naomiWedge"])
                return Visibility.Visible;
            return Visibility.Hidden;
        }
        if (SceneName == "E2_06C_RosaWedge")
        {
            if (saveVariables.ContainsKey("rosaWedge") && saveVariables["rosaWedge"] is bool && (bool)saveVariables["rosaWedge"])
                return Visibility.Visible;
            return Visibility.Hidden;
        }

        if (savedScene._chapter == _chapter)
        {
            var thisSceneName = SceneName;
            var thisChapterScenes = ScenesInChapter(chapter: _chapter);
            var savedSceneIndex = thisChapterScenes.FindIndex(scene => scene.SceneName == savedSceneName);
            var thisSceneIndex = thisChapterScenes.FindIndex(scene => scene.SceneName == thisSceneName);

            return thisSceneIndex < savedSceneIndex ? Visibility.Visible : Visibility.Locked;
        }

        // Scene is in a previous chapter
        // |
        // V
        // Fallthrough
        return Visibility.Visible;
    }

    public static List<SelectableScene> ScenesInChapter(int chapter)
    {
        return AllScenes.Where(scene => scene._chapter == chapter).ToList();
    }
    
    private static Dictionary<string, SelectableScene> _GenerateSceneNameToSceneMap()
    {
        var dict = new Dictionary<string, SelectableScene>();
        foreach (var scene in AllScenes)
        {
            dict[scene.SceneName] = scene;
        }
        return dict;
    }
    
    public static IEnumerator TestAndLogSceneVisibility()
    {
        if (Mgr_Saves.LoadState != Mgr_Saves.LoadStates.LoadDone)
            yield return new WaitForSeconds(1);
        ActuallyTestAndLogSceneVisibility();
    }
    
    private static void ActuallyTestAndLogSceneVisibility()
    {
        Plugin.LOG.LogInfo("Scene Count: " + AllScenes.Count);
        try
        {
            var data = Mgr_Saves.saveData;
            Dictionary<string, object> saveVariables;
            if (data == null || data.sceneName == null)
            {
                data = null;
                saveVariables = new Dictionary<string, object>();
            }
            else
            {
                saveVariables = OnyxUtils.ParseSaveDataString(data.globalVariables);
            }
            foreach (var scene in AllScenes)
            {
                var visibility = scene.SceneVisibility(data, saveVariables);
                Plugin.LOG.LogInfo("Scene " + scene.SceneName + " is " + visibility);
            }
        }
        catch (Exception e)
        {
            Plugin.LOG.LogError("Failed to test scene visibility: " + e);
            Plugin.LOG.LogError(e.StackTrace);
        }
    }

    private static List<SelectableScene> _GenerateAllScenes()
    {
        // lol
        // to auto-parse these from game memory would require initializing ink which 
        //      i do not want to spend the time screwing with. 
        // sadly, this will not account for scenes added via mods or anything like that.
        // perhaps at some point I can try to figure that out, but this is totally fine for now. 
        // Order of these == order of scenes in the game. IsValid checks depend on that! :)
        // https://open.spotify.com/track/3JBj5eXsEK1N9pCtga0Qx1?si=30c8830e8cee4e49
        var list = new List<SelectableScene>
        {
            new("E1_00_Beach", "The Beach", "", 1),
            new("E1_01_Morning", "Morning", "", 1),
            new("E1_MorningMusic_MG", "Morning Music", "", 1, overridenVisibility: Visibility.Hidden),
            new("E1_02_MeetTrish", "Meet Trish", "", 1),
            new("E1_03_Homeroom", "Homeroom", "", 1),
            new("E1_PhoneStaredown_MG", "Phone Staredown", "", 1, overridenVisibility: Visibility.Hidden),
            new("E1_FangsLogoDesign_MG", "Fangs Logo Design", "", 1, overridenVisibility: Visibility.Hidden),
            new("E1_04_MusicRoom", "Music Room", "", 1, minigameType: MinigameType.Music),
            new("E1_PerformingNewSong_MG", "Performing New Song", "", 1, overridenVisibility: Visibility.Hidden),
            new("E1_04A_ReedWedge", "Reed", "", 1),
            new("E1_04B_TrishWedge", "Trish", "", 1),
            new("E1_05_BusHome", "Bus Home", "", 1),
            new("E1_06_NaserandNaomi", "Naser and Naomi", "", 1, minigameType: MinigameType.Music),
            new("E1_PlayForNaomi_MG", "Play For Naomi", "", 1, overridenVisibility: Visibility.Hidden),
            new("E1_07_GroupChat", "Group Chat", "", 1),
            new("E1_08_Kitchen", "Kitchen", "", 1),
            new("E1_Spices_MG", "Spices", "", 1, overridenVisibility: Visibility.Hidden),
            new("E1_09_MeteorIntro", "The Meteor", "", 1),
            new("E2_01_MeteorAftermath", "Secret Admirer", "", 2),
            new("E2_Doomscroll_MG", "Doomscroll", "", 2, overridenVisibility: Visibility.Hidden),
            new("E2_02_NaserDrive", "Naser Drive", "", 2),
            new("E2_03_PreAssembly", "Pre Assembly", "", 2),
            new("E2_04_EmergencyMtg", "Emergency Meeting", "", 2),
            new("E2_05_CarpeDiem", "Carpe Diem", "", 2),
            new("E2_AuditoriumPeopleWatching_MG", "Auditorium People Watching", "", 2, overridenVisibility: Visibility.Hidden), 
            new("E2_06_YourDiems", "Your Diems", "", 2),
            new("E2_06B_NaomiWedge", "Naomi", "", 2),
            new("E2_06C_RosaWedge", "Rosa", "", 2),
            new("E2_07_MemoryBox", "Memory Box", "", 2),
            new("E2_08_FangSearches", "Fang Searches", "", 2),
            new("E2_ControllerHunt_MG", "Controller Hunt", "", 2, overridenVisibility: Visibility.Hidden),
            new("E2_09A_MidiMusic", "Midi Music", "", 2, minigameType: MinigameType.Music),
            new("E2_MidiMusicPerf_MG", "Midi Music Performance", "", 2, overridenVisibility: Visibility.Hidden),
            new("E2_10_PhotoDayAM", "Photo Day AM", "", 2),
            new("E2_11_KillMeNow", "Kill Me Now", "", 2),
            new("E2_PhotoDay_MG", "Photo Day", "", 2, overridenVisibility: Visibility.Hidden),
            new("E2_FindReed_MG", "Find Reed", "", 2, overridenVisibility: Visibility.Hidden),
            new("E2_12_RooftopReed", "Rooftop Reed", "", 2),
            new("E2_13_NaomiTheGenius", "Naomi The Genius", "", 2),
            new("E2_14_TheWalkHome", "The Walk Home", "", 2),
            new("E2_15_AuditionDay_A", "Audition Day A", "", 2),
            new("E2_15_AuditionDay_B", "Audition Day B", "", 2),
            new("E2_15_AuditionDay_C", "Audition Performance", "", 2, minigameType: MinigameType.Music),
            new("E2_AuditionPerf_MG", "Audition Performance", "", 2, overridenVisibility: Visibility.Hidden),
            new("E2_15_AuditionDay_D", "Audition Day D", "", 2),
            new("E3_01_CalderaDreams", "Caldera Dreams", "", 3),
            new("E3_PosterDesign_MG", "Poster Design", "", 3, overridenVisibility: Visibility.Hidden), 
            new("E3_02_BacktoReality", "Back to Reality", "", 3),
            new("E3_04_MeteorClass", "Meteor Class", "", 3),
            new("E3_04B_StellaWedge", "Stella Wedge", "", 3),
            new("E3_05_BOTBRampUp", "BOTB Ramp Up", "", 3),
            new("E3_06_HuntForMango", "Hunt For Mango", "", 3, minigameType: MinigameType.Worm),
            new("E3_HuntForMango_MG", "Hunt For Mango", "", 3, overridenVisibility: Visibility.Hidden),
            new("E3_07_BustedForPosters", "Busted For Posters", "", 3),
            new("E3_SecretAdmirerLyrics_MG", "Secret Admirer Lyrics", "", 3, overridenVisibility: Visibility.Hidden),
            new("E3_08_LnLIntro", "LnL Intro", "", 3),
            new("E3_09_LnL", "LnL", "", 3),
            new("E3_Books_MG", "Books", "", 3, overridenVisibility: Visibility.Hidden),
            new("E3_CrystalDoor_MG", "Crystal Door", "", 3, overridenVisibility: Visibility.Hidden),
            new("E3_10_BackToTheGarage", "Back To The Garage", "", 3),
            new("E4_01_CollegeApps", "College Apps", "", 4),
            new("E4_02_FutureIsSoon", "Future Is Soon", "", 4),
            new("E4_02B_SageWedge", "Sage Wedge", "", 4),
            new("E4_04_TimeToTry", "Time To Try", "", 4),
            new("E4_05_TheTriForce", "The Tri Force", "", 4),
            new("E4_06_RosaRoof", "Rosa Roof", "", 4),
            new("E4_07_TheOtherShoe", "The Other Shoe", "", 4),
            new("E4_08_LnL2", "LnL 2", "", 4),
            new("E4_09_TheShortGoodbye", "The Short Goodbye", "", 4),
            new("E4_10_BackHome", "Back Home", "", 4),
            new("E5_01_BreakfastTime", "Breakfast Time", "", 5),
            new("E5_NaserPose_MG", "Naser Pose", "", 5, overridenVisibility: Visibility.Hidden),
            new("E5_02_TrishWalk", "Trish Walk", "", 5),
            new("E5_SongwritingConvo_MG", "Songwriting Convo", "", 5, overridenVisibility: Visibility.Hidden),
            new("E5_03_TheNightBefore", "The Night Before", "", 5),
            new("E5_04_TheParentCall", "The Parent Call", "", 5),
            new("E5_05_RideAlong", "Ride Along", "", 5),
            new("E5_06_SetupTime", "Setup Time", "", 5),
            new("E5_07_GreenRoom", "Green Room", "", 5),
            new("E5_07C_NaserWedge", "Naser", "", 5),
            new("E5_08_BadVibesA", "Bad Vibes", "", 5),
            new("E5_08B_BadVibesB", "Fuck It", "", 5, minigameType: MinigameType.Music),
            new("E5_BotBPerformance_MG", "BOTB Performance", "", 5, overridenVisibility: Visibility.Hidden),
            new("E5_08C_BadVibesC", "Battle of the Bands", "", 5, minigameType: MinigameType.Music),
            new("E5_BotBPerformanceB_MG", "BOTB Performance B", "", 5, overridenVisibility: Visibility.Hidden),
            // < pr4
            new("E5_09_TheFight", "The Fight", "", 5),
            new("E5_10_TheAftermath", "The Aftermath", "", 5),
            new("E5_11_TheAurora", "The Aurora", "", 5),
            new("E6_01_ReedRooftop", "Going Away", "", 6),
            // < pr3
            new("E6_AftermathPerformance_MG", "Aftermath Performance", "", 6, overridenVisibility: Visibility.Hidden),
            new("E6_03_BeforeLnL", "LnL 3", "", 6),
            new("E6_04_LnL", "LnL", "", 6, overridenVisibility: Visibility.Hidden),
            new("E6_05_AfterLnL", "After LnL", "", 6),
            new("E7_01_BeachAlone", "Beach Alone", "", 7),
            new("E7_02_FriendsArrive", "Friends Arrive", "", 7),
            new("E7_03_TurnForWorse", "Turn For the Worse", "", 7),
            new("E7_04_Rosa", "Rosa", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_05_Stella", "Stella", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_06_Sage", "Sage", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_07_Intermission", "Intermission", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_08_Naser", "Naser", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_09_Reed", "Reed", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_10_Trish", "Trish", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_11_Naomi", "Naomi", "", 7, overridenVisibility: Visibility.Hidden),
            new("E7_12_TheRitual", "The Ritual", "", 7),
            new("E8_02_BeNotAlarmed", "Be Not Alarmed", "", 8),
            new("E8_04_FarewellLavaJava", "Farewell Lava Java", "", 8),
            new("E8_05_WormDramaCheckIn", "Worm Drama Check In", "", 8),
            new("E8_06_NaomiDate", "Naomi Date", "", 8),
            new("E8_NaomiSongwriting_MG", "Naomi Songwriting", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_07_NaomiHang", "Naomi Hang", "", 8),
            new("E8_08_BadNews", "Bad News", "", 8),
            // how much content got cut here oh my god
            // ;_;
            new("E8_10_TrishAndFang", "Trish And Fang", "", 8),
            new("E8_11_BeachDay", "Beach Day", "", 8),
            new("E8_12_TheLastDay", "The Last Day", "", 8),
            new("E8_13_OnArrival", "On Arrival", "", 8),
            new("E8_13_OnArrival_Alt", "On Arrival Alt", "", 8),
            new("E8_14_FinalPrep", "Final Prep", "", 8),
            new("E8_14_FinalPrep_Alt", "Final Prep Alt", "", 8),
            new("E8_15A_CalderaFest", "Caldera Fest", "", 8),
            new("E8_15A_CalderaFest_Alt", "Caldera Fest Alt", "", 8),
            // fuck you watch the whole thing 
            new("E8_15B_CalderaFest", "Caldera Fest", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_15B_CalderaFest_Alt", "Caldera Fest Alt", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_15C_CalderaFest", "Caldera Fest", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_15C_CalderaFest_Alt", "Caldera Fest Alt", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_Crowd_MG", "Crowd", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_FinalPerformance_MG", "Final Performance", "", 8, overridenVisibility: Visibility.Hidden),
            new("E8_FinalPerformance_Alt_MG", "Final Performance Alt", "", 8, overridenVisibility: Visibility.Hidden),
            new("Credits", "Credits", "", 8, overridenVisibility: Visibility.Visible) // idk where to put this? 8 i guess?
        };

        return list;
    }
}