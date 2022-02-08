//======================================
/*
@autor ktk.kumamoto
@date 2015.4.21 create
@note Button_ResetScene
*/
//======================================

using UnityEngine;
using System.Collections;

public class Button_ResetScene : MonoBehaviour {

	private string SceneName;

	void Start () {

		SceneName = Application.loadedLevelName;
	}

	void OnGUI()
	{
		if(GUI.Button(new Rect(10, 10, 100, 30), "ResetScene")){
				Application.LoadLevel(SceneName);
		}
	}
}