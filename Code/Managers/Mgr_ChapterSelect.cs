using System.Collections.Generic;
using UnityEngine;

namespace ChapterSelect.Code;


/**
 * Responsibilities:
 * - Dispatching callbacks during scene loads
 * 
 * 
 */
public class Mgr_ChapterSelect : MonoBehaviour
{
    private static Mgr_ChapterSelect _instance;
    public bool IsInThrowback = false;
    public static Mgr_ChapterSelect Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Mgr_ChapterSelect>();
            }

            return _instance;
        }
    }

    public string CurrentScene { get; set; } = null;
    
    public List<Koop.Notify> SceneReadyCallbacks { get; } = new() { };

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

    public void StartThrowbackForScene(string sceneName)
    {
        GameObject.Find("ChaptersMenu").GetComponent<ChaptersMenu>().Close(true);
        Mgr_CS_SaveManagement.Instance.AddSaveDisableReason("Throwback");
        // Mgr_Saves.saveData = new GameplaySaveData();
        IsInThrowback = true;
        Mgr_LevelFlow.Instance.LoadScene(sceneName);
    }
    
    public void EndThrowback()
    {
        IsInThrowback = false;
        Mgr_CS_SaveManagement.Instance.RemoveSaveDisableReason("Throwback");
    }

    /**
     * Dispatches callbacks.
     *
     * Marked this private as ideally it'd just be internally done by LevelFlow. A patch in LevelFlow calls this
     * through reflection, and it isn't meant to be called by anything else.
     */
    // ReSharper disable once UnusedMember.Local
    private void _SceneLoadedInternal()
    {
        foreach (var callback in SceneReadyCallbacks)
        {
            callback();
        }
    }
}