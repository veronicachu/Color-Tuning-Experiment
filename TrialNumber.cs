using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TrialNumber : MonoBehaviour {

    public GameObject textObject;
    private Text UItext;

    void Start()
    {
        UItext = textObject.GetComponent<Text>();
    }

	public void UpdateTrialNumber (int trialnum, int totalnum)
    {
        int temp = trialnum + 1;
        UItext.text = "Trial " + temp.ToString() + " of " + totalnum.ToString();
	}
}
