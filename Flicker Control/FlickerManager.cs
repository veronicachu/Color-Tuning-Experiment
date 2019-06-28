using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlickerManager : MonoBehaviour
{
    private ExpSetup m_ExpSetup;

    public float[] Frequencies = new float[2];

    [HideInInspector] public GameObject[] blueArray = new GameObject[2];
    [HideInInspector]
    public GameObject[] arrayHUD = new GameObject[2];

    void Start()
    {
        // Initialize script references
        m_ExpSetup = this.GetComponent<ExpSetup>();

        // Find all the HUD elements
        blueArray = GameObject.FindGameObjectsWithTag("BlueHUD");

        // Add all HUD elements to a single array
        for (int i = 0; i < 2; i++)
        {
            arrayHUD[i] = blueArray[i];
            //arrayHUD[i] = greenArray[i];
        }
    }

    //public void SetFlickerFreq(string targColor)
    //{
    //    List<bool> changeFreqB0 = m_ExpSetup.changeFreqB0;
    //    List<bool> changeFreqB1 = m_ExpSetup.changeFreqB1;
    //    List<bool> changeFreqB2 = m_ExpSetup.changeFreqB2;
    //    List<bool> changeFreqB3 = m_ExpSetup.changeFreqB3;
    //    List<bool> changeFreqB4 = m_ExpSetup.changeFreqB4;
    //    List<bool> changeFreqB5 = m_ExpSetup.changeFreqB5;
        
    //    if (targColor == "Blue0")
    //    {
    //        changeFreqB0 = ChangeFreqs(changeFreqB0);
    //    }
    //    else if (targColor == "Blue1")
    //    {
    //        changeFreqB1 = ChangeFreqs(changeFreqB1);
    //    }
    //    else if (targColor == "Blue2")
    //    {
    //        changeFreqB2 = ChangeFreqs(changeFreqB2);
    //    }
    //    else if (targColor == "Blue3")
    //    {
    //        changeFreqB3 = ChangeFreqs(changeFreqB3);
    //    }
    //    else if (targColor == "Blue4")
    //    {
    //        changeFreqB4 = ChangeFreqs(changeFreqB4);
    //    }
    //    else if (targColor == "Blue5")
    //    {
    //        changeFreqB5 = ChangeFreqs(changeFreqB5);
    //    }
    //}

    private List<bool> ChangeFreqs(List<bool> changeFreqList)
    {
        // select one of the flicker settings at random and remove from list
        int n = Random.Range(0, changeFreqList.Count);
        bool freqSetting = changeFreqList[n];
        changeFreqList.RemoveAt(n);

        if (freqSetting)
        {
            for (int i = 0; i < blueArray.Length; i++)
            {
                blueArray[i].GetComponent<FlickerControl>().Frequency = Frequencies[0];
            }
        }
        else if (!freqSetting)
        {
            for (int i = 0; i < blueArray.Length; i++)
            {
                blueArray[i].GetComponent<FlickerControl>().Frequency = Frequencies[1];
            }
        }

        return changeFreqList;
    }

    public void StartAllFlicker()
    {
        for (int i = 0; i < arrayHUD.Length; i++)
        {
            arrayHUD[i].GetComponent<FlickerControl>().StartFlicker();
        }
    }

    public void StopAllFlicker()
    {
        for (int i = 0; i < arrayHUD.Length; i++)
        {
            arrayHUD[i].GetComponent<FlickerControl>().StopFlicker();
        }
    }

}
