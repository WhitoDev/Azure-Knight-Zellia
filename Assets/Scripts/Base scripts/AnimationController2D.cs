using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;
using System.Reflection;

public class AnimationController2D : MonoBehaviour 
{
    public List<AnimationEffect> effects = new List<AnimationEffect>();

    public Animator animator
    {
        get
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
            return _animator;
        }
    }
    private Animator _animator;

    public void SetAnimationEffects()
    {
        var effectsByAnimationClip = effects.GroupBy(c => c.targetAnimationClip).Select(grp => grp.ToList()).ToList();
        
        foreach(var e in effectsByAnimationClip)
        {
            List<AnimationEvent> newEvents = new List<AnimationEvent>();
            foreach (AnimationEffect effect in e)
            {
                var length = effect.targetAnimationClip.length;
                var framerate = effect.targetAnimationClip.frameRate;
                var offset = 1 / framerate;

                var frameToPlayIn = Mathf.Clamp((effect.frameToPlayIn - 1), 0, effect.totalFramesInAnimation+1);

                float trueLenght = length - offset;
                float keyframeTime = trueLenght / (effect.totalFramesInAnimation - 1);

                var evnt = new AnimationEvent();
                evnt.time = Mathf.Clamp(keyframeTime * frameToPlayIn, 0, length);

                if(effect.prefab != null)
                    evnt.objectReferenceParameter = effect.prefab;

                if (!String.IsNullOrEmpty(effect.methodName))
                    evnt.functionName = effect.methodName;

                newEvents.Add(evnt);
            }
            AnimationUtility.SetAnimationEvents(e.First().targetAnimationClip, newEvents.ToArray());
        }
    }

    public void ClearAllAnimationEffects()
    {
        foreach(var animation in this.animator.runtimeAnimatorController.animationClips)
        {
            AnimationUtility.SetAnimationEvents(animation, new AnimationEvent[0]);
        }
    }

    public void DoNothing() { }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}

[CustomEditor(typeof(AnimationController2D))]
public class AnimationController2DEditor : Editor
{
    AnimationController2D myController;

    List<AnimationClip> animationClips;

    public override void OnInspectorGUI()
    {
        if (myController == null)
            myController = (AnimationController2D)target;

        if (animationClips == null)
        {
            var arr = myController.animator.runtimeAnimatorController.animationClips;
            if (arr != null)
                animationClips = arr.ToList();
        }

        if(animationClips == null || animationClips.Count == 0)
        {
            EditorGUILayout.LabelField("No animation clips found on this controller");
            return; 
        }

        string[] animationClipsNames = animationClips.Select(c => c.name).ToArray();
        
        Type myType = myController.GetType();
        var methodsInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        List<String> methodsNames = methodsInfo.ToList().Select(elm => elm.Name).ToList();

        foreach(AnimationEffect effect in myController.effects)
        {
            var prefab = EditorGUILayout.ObjectField(effect.prefab, typeof(GameObject)) as GameObject;
            
            effect.prefab = prefab;
            if (effect.targetAnimationClip != null)
            {
                int index = animationClips.FindIndex(c => c.name == effect.targetAnimationClip.name);                
                var selectedClipIndex = EditorGUILayout.Popup(index, animationClipsNames);
                if (animationClips[selectedClipIndex] != effect.targetAnimationClip)
                {
                    effect.targetAnimationClip = animationClips[selectedClipIndex];
                    return;
                }

                int methodNameIndex = !String.IsNullOrEmpty(effect.methodName) ? methodsNames.FindIndex(m => m == effect.methodName) : 0;
                var selectedMethodIndex = EditorGUILayout.Popup(methodNameIndex, methodsNames.ToArray());
                if (methodsNames[selectedMethodIndex] != effect.methodName)
                {
                    effect.methodName = methodsNames[selectedMethodIndex];
                    return;
                }

            }
            else
            {
                effect.targetAnimationClip = animationClips.FirstOrDefault();
                return;
            }


            var frameToPlayIn = EditorGUILayout.IntField("Frame to play in", effect.frameToPlayIn);
            var totalFramesInAnimation = EditorGUILayout.IntField("Number of frames on this animation", effect.totalFramesInAnimation);

            effect.frameToPlayIn = frameToPlayIn;
            effect.totalFramesInAnimation = totalFramesInAnimation;

            if (GUILayout.Button("Remove"))
            {
                myController.effects.Remove(effect);
                return;
            }
            GUILayout.Space(5);
        }

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Add effect"))
        {
            myController.effects.Add(new AnimationEffect() { targetAnimationClip = animationClips.FirstOrDefault(), methodName = methodsNames.FirstOrDefault()});
        }

        if(GUILayout.Button("Set animation effects"))
        {
            myController.SetAnimationEffects();
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Clear all effects"))
        {
            myController.ClearAllAnimationEffects();
        }
    }    
}
