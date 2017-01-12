using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

    #if UNITY_EDITOR
    public void SetAnimationEffects()
    {
        var effectsByAnimationClip = effects.GroupBy(c => c.targetAnimationClip).Select(grp => grp.ToList()).ToList();
        
        foreach(var e in effectsByAnimationClip)
        {
            List<AnimationEvent> newEvents = new List<AnimationEvent>();
            foreach (AnimationEffect effect in e)
            {
                var curveBindings = AnimationUtility.GetObjectReferenceCurveBindings(effect.targetAnimationClip);
                var spriteCurveBinding = curveBindings.First(c => c.type == typeof(SpriteRenderer));

                if(spriteCurveBinding != null)
                {
                    var keyframes = AnimationUtility.GetObjectReferenceCurve(effect.targetAnimationClip, spriteCurveBinding);
                    if (keyframes != null && keyframes.Length > 1)
                    {
                        int keyframeToPlayAt = Mathf.Clamp(effect.frameToPlayIn - 1, 0, keyframes.Length - 1);

                        float keyframeTime = keyframes[keyframeToPlayAt].time;
                        
                        if (effect.frameToPlayIn > keyframes.Length)
                            keyframeTime = effect.targetAnimationClip.length;

                        var evnt = new AnimationEvent();
                        evnt.time = keyframeTime;

                        switch(effect.selectedParameterType)
                        {
                            case AnimationEffect.parameterType.OBJECT:
                                if (effect.objectValue != null)
                                    evnt.objectReferenceParameter = effect.objectValue;
                                break;

                            case AnimationEffect.parameterType.BOOLEAN:
                                evnt.intParameter = effect.booleanValue;
                                break;

                            case AnimationEffect.parameterType.FLOAT:
                                evnt.floatParameter = effect.floatValue;
                                break;
                        }                        

                        if (!String.IsNullOrEmpty(effect.methodName))
                            evnt.functionName = effect.methodName;

                        newEvents.Add(evnt);
                    }
                }
            }

            newEvents = newEvents.OrderBy(c => c.time).ToList();

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

    #endif

    private GameObject InstantiateObjectBase(GameObject obj, Transform transformAnchor)
    {
        var objPos = obj.transform.position;

        Vector3 objPositionOffset = Vector3.zero;
        if (transformAnchor != null)
            objPositionOffset = new Vector3(objPos.x * (Mathf.Sign(transformAnchor.localScale.x) == 1 ? 1 : -1), objPos.y, objPos.z);

        Vector3 position = this.transform.position + objPositionOffset;

        var objLocalScale = obj.transform.localScale;
        
        Vector3 newlocalScale = objLocalScale;
        if(transformAnchor != null)
            newlocalScale = new Vector3(objLocalScale.x * (Mathf.Sign(transformAnchor.localScale.x) == 1 ? 1 : -1), objLocalScale.y, objLocalScale.z);

        var newObj = Instantiate(obj, position, Quaternion.identity) as GameObject;
        newObj.transform.localScale = newlocalScale;

        return newObj;
    }

    public void InstantiateObject(GameObject obj)
    {
        InstantiateObjectBase(obj, null);
    }

    public void InstantiateObjectFromThis(GameObject obj)
    {
        InstantiateObjectBase(obj, this.transform);
    }

    public void InstantiateObjectFromParent(GameObject obj)
    {
        InstantiateObjectBase(obj, this.transform.parent);
    }

    public void InstantiateObjectAsChild(GameObject obj)
    {
        var newObj = InstantiateObjectBase(obj, null);
        newObj.transform.parent = this.transform.parent;
    }

    public void InstantiateObjectAsChildFromThis(GameObject obj)
    {
        var newObj = InstantiateObjectBase(obj, this.transform);
        newObj.transform.parent = this.transform.parent;
    }

    public void InstantiateObjectAsChildFromParent(GameObject obj)
    {
        var newObj = InstantiateObjectBase(obj, this.transform.parent);
        newObj.transform.parent = this.transform.parent;
    }

    public void DoNothing() { }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }


}

#if UNITY_EDITOR
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
        var methodsInfo = myType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        List<String> methodsNames = methodsInfo.ToList().Select(elm => elm.Name).ToList();

        foreach(AnimationEffect effect in myController.effects)
        {
            effect.fold = EditorGUILayout.Foldout(effect.fold, effect.targetAnimationClip.name + " // " + effect.methodName);

            if (effect.fold)
            {
                effect.selectedParameterType = (AnimationEffect.parameterType)EditorGUILayout.EnumPopup("Parameter type", effect.selectedParameterType);

                switch(effect.selectedParameterType)
                {
                    case AnimationEffect.parameterType.OBJECT:
                        var prefab = EditorGUILayout.ObjectField(effect.objectValue, typeof(GameObject)) as GameObject;
                        effect.objectValue = prefab;
                        break;

                    case AnimationEffect.parameterType.BOOLEAN:
                        var boolValue = EditorGUILayout.Toggle(effect.booleanValue == -1 ? false : true) == true ? 1 : -1;
                        effect.booleanValue = boolValue;
                        break;

                    case AnimationEffect.parameterType.FLOAT:
                        var floatValue = EditorGUILayout.FloatField(effect.floatValue);
                        effect.floatValue = floatValue;
                        break;
                }                

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

                effect.frameToPlayIn = frameToPlayIn;

                if (GUILayout.Button("Remove"))
                {
                    myController.effects.Remove(effect);
                    return;
                }
                GUILayout.Space(5);
            }
        }

        EditorGUILayout.BeginHorizontal();

        if(GUILayout.Button("Add effect"))
        {
            myController.effects.Add(new AnimationEffect() { 
                targetAnimationClip = animationClips.FirstOrDefault(), 
                methodName = methodsNames.FirstOrDefault(), 
                selectedParameterType = AnimationEffect.parameterType.OBJECT});
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
#endif