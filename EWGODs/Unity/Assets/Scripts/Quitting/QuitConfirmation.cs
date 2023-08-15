using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitConfirmation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       transform.Find("Yes").gameObject.GetComponent<Button>().onClick.AddListener(OnYesClicked);
	   transform.Find("No").gameObject.GetComponent<Button>().onClick.AddListener(OnNoClicked);
    }

    void OnYesClicked()
	{
		Application.Quit();
	}
	
	void OnNoClicked()
	{
		gameObject.SetActive(false);
	}
}
