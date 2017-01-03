using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    private static float timeScaleNormal;

    public static bool gamePaused;
    public static GameObject AfterImage;

    void Start()
    {
        timeScaleNormal = Time.timeScale;
    }

    public static void PauseGame()
    {
        Time.timeScale = 0;
        gamePaused = true;
    }

    public static void UnpauseGame()
    {
        Time.timeScale = timeScaleNormal;
        gamePaused = false;
    }
}
