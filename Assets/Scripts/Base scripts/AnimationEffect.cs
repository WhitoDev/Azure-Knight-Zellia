using UnityEngine;
using System.Collections;

[System.Serializable]
public class AnimationEffect 
{
    public enum parameterType
    {
        BOOLEAN = 1,
        OBJECT = 2,
        FLOAT = 3
    }
    public bool fold = true;

    public parameterType selectedParameterType;
    public GameObject objectValue;
    public int booleanValue;
    public float floatValue;
    public AnimationClip targetAnimationClip;
    public int frameToPlayIn;
    public string methodName;
}
