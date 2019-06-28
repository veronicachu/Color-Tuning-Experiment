using UnityEngine;
using System.Collections.Generic;

public class ExpCue_Exp : MonoBehaviour
{
    //[HideInInspector]
    public GameObject activeTarget;

    private GameObject cueClone;
    private ExpSetup_Exp m_ExpSetup;
    private Vector3 fixationLoc;

    private GameObject[] mainPeripheral;
    private GameObject[] oppPeripheral;

    void Start()
    {
        m_ExpSetup = this.GetComponent<ExpSetup_Exp>();
        fixationLoc = GameObject.Find("FixationWaypoint").transform.position;

        mainPeripheral = GameObject.FindGameObjectsWithTag("RedHUD");   // main peripheral
        oppPeripheral = GameObject.FindGameObjectsWithTag("BlueHUD");   // opposite peripheral
    }

    public GameObject ShowCue()
    {
        // get possible targets
        List<GameObject> targetTrials = m_ExpSetup.targetTypeTrials;

        // select one of the target types at random
        int n = Random.Range(0, targetTrials.Count);
        activeTarget = targetTrials[n];
        targetTrials.RemoveAt(n);

        // display cue in front of subject
        cueClone = Instantiate(activeTarget, fixationLoc, activeTarget.transform.rotation) as GameObject;
        cueClone.tag = "Clone";

        return cueClone;
    }

    public Material SetPeripheral()
    {
        // get current list of target trials
        List<Material> pTrials = m_ExpSetup.mainPeripheralTrials;
        Debug.Log("There are " + pTrials.Count + " trials left.");

        // select one of the trials at random and remove from the list of target trials
        int n = Random.Range(0, pTrials.Count);
        Material mainPeripheralMat = pTrials[n];
        pTrials.RemoveAt(n);

        // get opposite peripheral
        Dictionary<Material, Material> pPairs = m_ExpSetup.oppPeripheral;
        Material oppPeripheralMat = pPairs[mainPeripheralMat];

        // set freqs
        for (int i = 0; i < mainPeripheral.Length; i++)
        {
            mainPeripheral[i].GetComponent<Renderer>().material = mainPeripheralMat;
            mainPeripheral[i].GetComponent<FlickerControl>().materials[0] = mainPeripheralMat;
        }
        for (int i = 0; i < oppPeripheral.Length; i++)
        {
            oppPeripheral[i].GetComponent<Renderer>().material = oppPeripheralMat;
            oppPeripheral[i].GetComponent<FlickerControl>().materials[0] = oppPeripheralMat;
        }

        return mainPeripheralMat;
    }
}
