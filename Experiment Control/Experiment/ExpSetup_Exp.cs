using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ExpSetup_Exp : MonoBehaviour
{
    public int totalTrials;
    public int[] numTrials;

    public List<GameObject> targetTypes = new List<GameObject>();
    public List<Material> mainPeripheralTypes = new List<Material>();
    public List<Material> oppPeripheralTypes = new List<Material>();

    [HideInInspector]
    public List<GameObject> targetTypeTrials = new List<GameObject>();
    [HideInInspector]
    public List<Material> mainPeripheralTrials = new List<Material>();
    public Dictionary<Material, Material> oppPeripheral = new Dictionary<Material, Material>();

    public void Awake()
    {
        // create equal numbers of peripheral trials
        //int trialsPerPeripheral = totalTrials / mainPeripheralTypes.Count;
        //for (int j = 0; j < trialsPerPeripheral; j++)
        //{
        //    for (int i = 0; i < mainPeripheralTypes.Count; i++)
        //    {
        //        mainPeripheralTrials.Add(mainPeripheralTypes[i]);
        //    }
        //}
        for (int i = 0; i < mainPeripheralTypes.Count; i++)
        {
            for (int j = 0; j < numTrials[i]; j++)
            {
                mainPeripheralTrials.Add(mainPeripheralTypes[i]);
            }
        }

        if(mainPeripheralTrials.Count != totalTrials)
        {
            Text errorText = GameObject.Find("Instructions").GetComponent<Text>();
            errorText.text = "Trials Don't Match";
        }

        // create equal numbers of target type trials
        int trialsPerTargetType = totalTrials / targetTypes.Count;
        for (int j = 0; j < trialsPerTargetType; j++)
        {
            for (int i = 0; i < targetTypes.Count; i++)
            {
                targetTypeTrials.Add(targetTypes[i]);
            }
        }
        // if not enough target type trials to match with peri, then make one more set (mainly for practice scene)
        if(targetTypeTrials.Count < mainPeripheralTrials.Count)
        {
            for (int i = 0; i < targetTypes.Count; i++)
            {
                targetTypeTrials.Add(targetTypes[i]);
            }
        }

        // create dictionary linking main peripheral colors to opposite peripheral colors
        for (int j = 0; j < mainPeripheralTypes.Count; j++)
        {
            oppPeripheral.Add(mainPeripheralTypes[j], oppPeripheralTypes[j]);
        }
    }
}
