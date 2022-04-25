using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;

public class RECORDING_OF_LIGHTING : MonoBehaviour
{
    [SerializeField] Transform RECORD_TARGET;
    [SerializeField] string RECORD_DATA;
    [SerializeField] string DIRECTORY_PATH;

    private Quaternion rota;//Reccord Data
    private string path;

    private void Start()
    {      
        //path = @"C:\Users\futur\VritualLive_TechnicalArtist\Assets\DataBuffor"
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        
    }
}

public class GETTING_DIRECTORY_PATH : EditorWindow
{
    public string p;
}
