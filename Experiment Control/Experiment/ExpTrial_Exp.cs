using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpTrial_Exp : MonoBehaviour
{
    public int targetNum;
    public int distractorNum;
    
    public List<Material> redMaterials = new List<Material>();
    public List<Material> greenMaterials = new List<Material>();
    public List<Material> blueMaterials = new List<Material>();

    // for placing fixation cross
    public GameObject fixationCross;
    private Vector3 fixationCrossPos;
    private GameObject fixationClone;

    // for array locations
    [HideInInspector] public List<Transform> waypoints = new List<Transform>();
    [HideInInspector] public Transform targetWaypoint;

    // distractor lists
    [HideInInspector]
    public List<GameObject> targetList = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> distractorList1 = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> distractorList2 = new List<GameObject>();

    // for storing locations into a file
    [HideInInspector] public GameObject[] arrayItems;

    private int subsetNum;   // number of total items in search array

    // for switching cases
    private string targetTag;

    void Start()
    {
        // Get the fixation waypoint game object's location
        fixationCrossPos = GameObject.Find("FixationWaypoint").transform.position;

        subsetNum = targetNum + distractorNum;
    }

    public void SpawnShapes(int targetSignal)
    {
        // Instantiate a copy of the fixation gameobject in the fixation waypoint location
        fixationClone = Instantiate(fixationCross, fixationCrossPos, Quaternion.identity) as GameObject;
        fixationClone.tag = "Clone";

        // Collect all of the possible object locations
        GameObject[] arrayObjects = GameObject.FindGameObjectsWithTag("Waypoints");

        // Transform the object location array object to a list object
        // Remove the fixation location from the list of possible search array locations
        int removeWaypoint = 100;   // !!hard-coded to remove fixation location from possible item placement locations!!
        waypoints.Clear();
        for (int i = 0; i < arrayObjects.Length; i++)
        {
            waypoints.Add(arrayObjects[i].transform);

            if (waypoints[i].name == "Waypoint33")
                removeWaypoint = i;
        }
        //waypoints.RemoveAt(removeWaypoint);

        // Set up the gameobject search array
        GameObject[] searchArray = new GameObject[subsetNum];

        // Get the current target
        GameObject target = GetComponent<ExpCue_Exp>().activeTarget;

        // Collect the possible target and distractor game objects
        // Targets and distractors depend on the trial's target and gameobjects must be tagged in the Unity editor
        // Only coded with red, green, and blue gameobjects in mind
        targetList.Clear();
        distractorList1.Clear();
        distractorList2.Clear();
        if (target.tag[0] == 'G')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList1.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            distractorList2.AddRange(GameObject.FindGameObjectsWithTag("Red"));
        }
        else if (target.tag[0] == 'B')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList1.AddRange(GameObject.FindGameObjectsWithTag("Red"));
            distractorList2.AddRange(GameObject.FindGameObjectsWithTag("Green"));
        }
        else if (target.tag[0] == 'R')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList1.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            distractorList2.AddRange(GameObject.FindGameObjectsWithTag("Green"));
        }

        // Removes the current target orientation from the list of distractors
        for (int i = 0; i < targetList.Count; i++)
        {
            if (targetList[i].name == target.name)
                targetList.RemoveAt(i);
        }

        // If target present in current sub-trial, place target randomly on one of the possible locations and 
        // tag instantiated target gameobject as corresponding color case
        if (targetSignal == 0)
        {
            // Select random waypoint location
            int targetIndex = Random.Range(0, waypoints.Count);
            targetWaypoint = waypoints[targetIndex];

            // Place target clone and establish name for switch-case
            searchArray[0] = Instantiate(target, targetWaypoint.position, target.transform.rotation) as GameObject;
            waypoints.RemoveAt(targetIndex);
            targetTag = target.tag;
        }

        // If target absent in current sub-trial, place target in the "No Target Waypoint" location and label 
        // as "NoTarget" case
        if (targetSignal > 0)
        {
            // Get the no target waypoint location
            targetWaypoint = GameObject.Find("No Target Waypoint").transform;

            // Place and tag target clone and establish name for switch-case
            GameObject noTarget = Instantiate(target, targetWaypoint.position, target.transform.rotation) as GameObject;
            noTarget.tag = "Clone";
            targetTag = "NoTarget";
        }

        //int tempSubsetNum = subsetNum;      // store subsetNum into a temp that is useable
        List<Material> tempgreenMaterials = new List<Material>(greenMaterials);
        List<Material> tempblueMaterials = new List<Material>(blueMaterials);

        // Place distractors randomly in the possible when no target is present
        if (targetTag == "NoTarget")
        {
            //targetNum = tempSubsetNum / 2;
            //distractorNum = tempSubsetNum / 2;
            
            // Target color objects - no target orientation
            for (int i = 0; i < targetNum; i++)
            {
                int distIndex = Random.Range(0, targetList.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i] = Instantiate(targetList[distIndex], distWaypoint.position, targetList[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);
            }
            // Distractor 1 color objects
            for (int i = targetNum; i < targetNum + distractorNum/2; i++)
            {
                int distIndex = Random.Range(0, distractorList1.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i] = Instantiate(distractorList1[distIndex], distWaypoint.position, distractorList1[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);

                // Randomly select distractor luminance and saturation changed materials
                    int n = Random.Range(0, tempblueMaterials.Count);
                    searchArray[i].GetComponent<Renderer>().material = tempblueMaterials[n];
                    tempblueMaterials.RemoveAt(n);
            }
            // Distractor 2 color objects
            for (int i = targetNum + distractorNum / 2; i < targetNum + distractorNum; i++)
            {
                int distIndex = Random.Range(0, distractorList2.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i] = Instantiate(distractorList2[distIndex], distWaypoint.position, distractorList2[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);

                // Randomly select distractor luminance and saturation changed materials
                    int n = Random.Range(0, tempgreenMaterials.Count);
                    searchArray[i].GetComponent<Renderer>().material = tempgreenMaterials[n];
                    tempgreenMaterials.RemoveAt(n);
            }
            arrayItems = searchArray;
        }
        // Place distractors randomly in the remaining locations when a target has already been placed
        else
        {
            //tempSubsetNum = tempSubsetNum - 1;
            //targetNum = tempSubsetNum / 2;
            //distractorNum = tempSubsetNum - targetNum;
            int temptargetNum = targetNum - 1;
            
            // Target color objects - no target orientation
            for (int i = 0; i < temptargetNum; i++)
            {
                int distIndex = Random.Range(0, targetList.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i + 1] = Instantiate(targetList[distIndex], distWaypoint.position, targetList[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);
            }
            // Distractor 1 color objects
            for (int i = temptargetNum; i < temptargetNum + distractorNum/2; i++)
            {
                int distIndex = Random.Range(0, distractorList1.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i + 1] = Instantiate(distractorList1[distIndex], distWaypoint.position, distractorList1[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);

                // Randomly select distractor luminance and saturation changed materials
                    int n = Random.Range(0, tempblueMaterials.Count);
                    searchArray[i + 1].GetComponent<Renderer>().material = tempblueMaterials[n];
                    tempblueMaterials.RemoveAt(n);
            }
            // Distractor 2 color objects
            for (int i = temptargetNum + distractorNum/2; i < temptargetNum + distractorNum ; i++)
            {
                int distIndex = Random.Range(0, distractorList2.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i + 1] = Instantiate(distractorList2[distIndex], distWaypoint.position, distractorList2[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);

                // Randomly select distractor luminance and saturation changed materials
                    int n = Random.Range(0, tempgreenMaterials.Count);
                    searchArray[i + 1].GetComponent<Renderer>().material = tempgreenMaterials[n];
                    tempgreenMaterials.RemoveAt(n);
            }
            arrayItems = searchArray;
        }

        // Tag all the objects as "Clone" for easy deletion later
        for (int i = 0; i < searchArray.Length; i++)
        {
            searchArray[i].transform.parent = transform;
            searchArray[i].tag = "Clone";
        }
    }
}
