using System.Collections.Generic;
using UnityEngine;

namespace ChapterSelect.Code;

public class Mgr_CS_SaveManagement : MonoBehaviour
{
    public bool SavesEnabled => _saveDisableReasons.Count == 0;
    
    private readonly HashSet<string> _saveDisableReasons = new() { };
    private bool _lastSaveState = true;

    private readonly GameplaySaveData _backupSaveData = new GameplaySaveData();

    private static Mgr_CS_SaveManagement _instance;
    public static Mgr_CS_SaveManagement Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Mgr_CS_SaveManagement>();
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

    private void SaveDisableReasonsUpdated()
    {
        switch (SavesEnabled)
        {
            // SM => Saves Disabled
            case false when _lastSaveState:
                _backupSaveData.lastSavedDateTime = Mgr_Saves.saveData.lastSavedDateTime;
                _backupSaveData.sceneName = Mgr_Saves.saveData.sceneName;
                _backupSaveData.socialMediaData = Mgr_Saves.saveData.socialMediaData;
                _backupSaveData.globalVariables = Mgr_Saves.saveData.globalVariables;
                _backupSaveData.collectedPhotos = Mgr_Saves.saveData.collectedPhotos;
                _backupSaveData.unviewedPhotos = Mgr_Saves.saveData.unviewedPhotos;
                _backupSaveData.collectedFlashbacks = Mgr_Saves.saveData.collectedFlashbacks;
                _backupSaveData.unviewedFlashbacks = Mgr_Saves.saveData.unviewedFlashbacks;
                _backupSaveData.previousSceneHistory = Mgr_Saves.saveData.previousSceneHistory;
                _backupSaveData.socialMediaNotif = Mgr_Saves.saveData.socialMediaNotif;
                break;
            // SM => Saves Enabled
            case true when !_lastSaveState:
                Mgr_Saves.saveData.lastSavedDateTime = _backupSaveData.lastSavedDateTime;
                Mgr_Saves.saveData.sceneName = _backupSaveData.sceneName;
                Mgr_Saves.saveData.socialMediaData = _backupSaveData.socialMediaData;
                Mgr_Saves.saveData.globalVariables = _backupSaveData.globalVariables;
                Mgr_Saves.saveData.collectedPhotos = _backupSaveData.collectedPhotos;
                Mgr_Saves.saveData.unviewedPhotos = _backupSaveData.unviewedPhotos;
                Mgr_Saves.saveData.collectedFlashbacks = _backupSaveData.collectedFlashbacks;
                Mgr_Saves.saveData.unviewedFlashbacks = _backupSaveData.unviewedFlashbacks;
                Mgr_Saves.saveData.previousSceneHistory = _backupSaveData.previousSceneHistory;
                Mgr_Saves.saveData.socialMediaNotif = _backupSaveData.socialMediaNotif;
                break;
        }

        _lastSaveState = SavesEnabled;
    }
    
    public void AddSaveDisableReason(string reason)
    {
        if (_saveDisableReasons.Contains(reason))
        {
            Plugin.LOG.LogWarning($"Save disable reason {reason} already exists!!");
        }
        _saveDisableReasons.Add(reason);
        SaveDisableReasonsUpdated();
    }
    
    public void RemoveSaveDisableReason(string reason)
    {
        if (!_saveDisableReasons.Contains(reason))
        {
            Plugin.LOG.LogWarning($"Save disable reason {reason} does not exist!!");
        }
        _saveDisableReasons.Remove(reason);
        SaveDisableReasonsUpdated();
    }
}