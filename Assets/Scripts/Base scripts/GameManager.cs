using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public static GameManager instance = null;

    public float timeScaleNormal;
    public bool gamePaused;
    public GameObject AfterImage;
    private Coroutine SleepCorutine;

    void Start()
    {
        //PlayerPrefs.DeleteAll();
    }

    void Awake()
    {
        instance = this;
        timeScaleNormal = Time.timeScale;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        gamePaused = true;
    }

    public void UnpauseGame()
    {
        Time.timeScale = timeScaleNormal;
        gamePaused = false;
    }

    public void PlayerMoved()
    {
        var platforms = FindObjectsOfType<PlatformController>();
        
        foreach(PlatformController platformController in platforms)
        {
            platformController.MovePassanger(false);
        }
    }

    public void SleepGame(float seconds, bool overridePrevious)
    {
        if (overridePrevious)
        {
            if (SleepCorutine != null)
            {
                StopCoroutine(SleepCorutine);
                SleepCorutine = null;
            }
            StartCoroutine(Sleep(seconds));
        }

        else
        {
            if(SleepCorutine == null)
            {
                StartCoroutine(Sleep(seconds));
            }
        }
    }

    IEnumerator Sleep(float seconds)
    {
        float start = Time.realtimeSinceStartup;

        PauseGame();

        while(Time.realtimeSinceStartup < start + seconds)
        {
            yield return null;
        }

        UnpauseGame();
    }
}
