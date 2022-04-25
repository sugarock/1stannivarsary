using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Animations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEditor.Animations;


public class ANIMATIONKEY_SETLIST : MonoBehaviour
{
    [SerializeField] private GameObject light_obj;
    [SerializeField] private string anim_name;
    [SerializeField] private Renderer _renderer;
    private Keyframe strkey = new Keyframe(0,0);
    private Keyframe endkey = new Keyframe(1,1);

    private AnimatorController _animator;
    
    private AnimationClip clip;
    //private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        clip = new AnimationClip();
        clip.name = anim_name;

        AnimationCurve animationCurve = new AnimationCurve();
        animationCurve.AddKey(strkey);
        animationCurve.AddKey(endkey);
        clip.SetCurve("", typeof(MeshRenderer), "Element 0", animationCurve);
        //light_obj.gameObject.GetComponent<Animation>().AddClip(clip,clip.name);
        AssetDatabase.CreateAsset(clip, @"Assets\Script\Utillity\Animation\" + anim_name + ".anim");
        //clip.AddEvent(k_flame);
        //material.SetColor("Color", new Color(1.0f, 0.0f, 1.0f, 1.0f));
        //material.SetFloat("Width", Mathf.Clamp(0, -0.07f, 0.5f));
        //render material color
        //_renderer.material.color = ;
        Color color = (_renderer.material.GetColor("_Color"));
        Debug.Log(color * 255f);
        Debug.Log(_renderer.material.GetFloat("_ConeWidth"));
        _animator.AddMotion(clip);
        Color w = Color.green;
        _renderer.material.SetColor("_Color", w);
    }

    // Update is called once per frame

    void Update()
    {
        
    }
}
