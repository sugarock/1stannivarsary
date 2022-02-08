//======================================
/*
@autor ktk.kumamoto
@date 2015.4.20 create
@note Button_TimeStop
*/
//======================================

using UnityEngine;
using System.Collections;

public class Button_TimeStop : MonoBehaviour {
	
	private bool isChecked = true;
	private string Time_State = "Start";
	
	void Update() {
		if (Input.GetKeyDown("space")){
			if(isChecked == true){
				isChecked = false;
			}else{
				isChecked = true;
			}
		}
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 300, 20), "SpaceKey: switching the start and pause");
		Rect rect1 = new Rect(10, 30, 400, 30);
		isChecked = GUI.Toggle(rect1, isChecked, Time_State);
		if (isChecked ) {
			Time_State = "Start";
			Time.timeScale = 1.0F;
		} else {
			Time_State = "Pause";
			Time.timeScale = 0.0F;
		}
	}
}