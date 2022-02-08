using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMesh : MonoBehaviour {
    public Transform Parent;
    public Dropdown dropdown;
    public List<GameObject> ObjChangeButton = new List<GameObject>();
    public List<GameObject> AllButton = new List<GameObject>();

    private Dictionary<string, GameObject[]> _DicAllList = new Dictionary<string, GameObject[]>();
    private List<GameObject> _MeshList = new List<GameObject>();
    private GameObject _currentMesh;
    private void Awake()
    {
        GameObject p = GameObject.Find("Player");
        p.SetActive(false);
        
        List<string> DataFolderName = new List<string>();

        string path = Application.dataPath + "\\Resources\\Prefaps";
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
        foreach(var pair in di.GetDirectories())
        {
            DataFolderName.Add(pair.Name);
        }


        if (DataFolderName != null)
        {            
            dropdown.ClearOptions();
            List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
            foreach (var pair in DataFolderName)
            {
                GameObject[] obj = Resources.LoadAll<GameObject>("Prefaps/" + pair);
                if (obj != null)
                {
                    _DicAllList.Add(pair, obj);
                }
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = pair;
                list.Add(data);
            }
            dropdown.AddOptions(list);
        }
        DropdownValueChanged(dropdown);
    }

    public void DropdownValueChanged(Dropdown change)
    {
        while(Parent.childCount > 0)
        {
            DestroyImmediate(Parent.GetChild(0).gameObject);
        }

        string key = change.options[change.value].text;
        if (_DicAllList.ContainsKey(key))
        {
            _MeshList.Clear();
            for (int i=0; i<_DicAllList[key].Length; i++)
            {
                GameObject obj = GameObject.Instantiate(_DicAllList[key][i], Parent);
                _MeshList.Add(obj);
            }
            OnClickMeshChange(0);
        }
        for (int i=0; i< ObjChangeButton.Count; i++)
        {
            ObjChangeButton[i].SetActive(i < _MeshList.Count);
        }
    }

    public void OnClickMeshChange(int num)
    {
        for (int i=0; i< _MeshList.Count; i++)
        {
            _MeshList[i].transform.localPosition = i == num ? Vector3.zero : new Vector3(1000f, 0, 0);
            if (i == num)
                _currentMesh = _MeshList[i];
        }

        if (_currentMesh != null)
        {
            Animator anim = _currentMesh.GetComponent<Animator>();
            if (anim != null)
            {
                bool isFirst = true;

                AnimationClip[] infos = anim.runtimeAnimatorController.animationClips;
                List<string> names = new List<string>();
                foreach(var pair in infos)
                {
                    names.Add(pair.name);
                }
                for (int i = 0; i < AllButton.Count; i++)
                {
                    AllButton[i].SetActive(names.Exists(val => val == AllButton[i].name));

                    if (isFirst && AllButton[i].activeInHierarchy)
                    {
                        isFirst = false;
                        OnClickButton(AllButton[i]);
                    }
                }
            }            
        }
    }

    public void OnClickButton(GameObject obj)
    {
        string aniName = obj.name;
        Animator anim = _currentMesh.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play(aniName);
        }
    }
}
