using UnityEngine;
using System.Collections;

public class Button_ChangeEffect : MonoBehaviour {
	
	public GameObject CreateEffect;
	public int num = 0;

	void Update() {
		if (Input.GetKeyDown("right"))
		{
			CreateEffect.SendMessage("SetEff_Change");
			num ++;
			if(num == 7)
			{
				num = 0;
			}

		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(420, 15, 500, 20), "Right:Key   Effect" + num);
		if(GUI.Button(new Rect(310, 10, 100, 30), "ChangeEffect")){
			CreateEffect.SendMessage("SetEff_Change");
			num ++;
			if(num == 7)
			{
				num = 0;
			}

		}
	}
}
