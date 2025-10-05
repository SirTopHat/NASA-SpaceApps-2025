using NasaSpaceApps.FarmFromSpace.Game.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void QuitGame() { 
        Application.Quit();
    }

    public void DeleteSaveData() { 
        GameSaveManager.DeleteAllGameData();
        GameSaveManager.DeleteEndlessModeSave();
        SoundManager.Instance.PlaySfx("coin");
    }
}
