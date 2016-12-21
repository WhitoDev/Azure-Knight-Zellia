using UnityEngine;
using System.Collections;

[System.Serializable]
public class AnimationEffect 
{
    public GameObject prefab;
    public AnimationClip targetAnimationClip;
    public int frameToPlayIn;
    public int totalFramesInAnimation;
    public string methodName;
}
