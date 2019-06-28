using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpTrial_Control : MonoBehaviour
{
    public int subsetNum;   // number of total items in search array
    public List<Material> allMaterials = new List<Material>();

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
    public List<GameObject> distractorList = new List<GameObject>();

    // for storing locations into a file
    [HideInInspector] public GameObject[] arrayItems;

    // for storing the number of red and green distractors
    private int targetNum;
    private int distractorNum;

    // for switching cases
    private string targetTag;

    void Start()
    {
        // Get the fixation waypoint game object's location
        fixationCrossPos = GameObject.Find("FixationWaypoint").transform.position;
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
        waypoints.RemoveAt(removeWaypoint);

        // Set up the gameobject search array
        GameObject[] searchArray = new GameObject[subsetNum];

        // Get the current target
        GameObject target = GetComponent<ExpCue_Exp>().activeTarget;

        // Collect the possible target and distractor game objects
        // Targets and distractors depend on the trial's target and gameobjects must be tagged in the Unity editor
        // Only coded with red, green, and blue gameobjects in mind
        targetList.Clear();
        distractorList.Clear();
        if (target.tag[0] == 'G')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Red"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Yellow"));
        }
        else if (target.tag[0] == 'B')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Red"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Green"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Yellow"));
        }
        else if (target.tag[0] == 'R')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Green"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Yellow"));
        }
        else if (target.tag[0] == 'Y')
        {
            targetList.AddRange(GameObject.FindGameObjectsWithTag(target.tag));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Blue"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Green"));
            distractorList.AddRange(GameObject.FindGameObjectsWithTag("Red"));
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

            // Randomly select material hue/lum
            int matIndex = Random.Range(0, allMaterials.Count);
            searchArray[0].GetComponent<Renderer>().material = allMaterials[matIndex];
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

        int tempSubsetNum = subsetNum;      // store subsetNum into a temp that is useable

        // Place distractors randomly in the possible when no target is present
        if (targetTag == "NoTarget")
        {
            // Distractor color objects
            for (int i = 0; i < tempSubsetNum; i++)
            {
                int distIndex = Random.Range(0, distractorList.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i] = Instantiate(distractorList[distIndex], distWaypoint.position, distractorList[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);

                // Randomly select material hue/lum
                int matIndex = Random.Range(0, allMaterials.Count);
                searchArray[i].GetComponent<Renderer>().material = allMaterials[matIndex];
            }
            arrayItems = searchArray;
        }
        // Place distractors randomly in the remaining locations when a target has already been placed
        else
        {
            tempSubsetNum = tempSubsetNum - 1;

            // Distractor color objects
            for (int i = 0; i < tempSubsetNum; i++)
            {
                int distIndex = Random.Range(0, distractorList.Count);
                int tempIndex = Random.Range(0, waypoints.Count);
                Transform distWaypoint = waypoints[tempIndex];
                searchArray[i + 1] = Instantiate(distractorList[distIndex], distWaypoint.position, distractorList[distIndex].transform.rotation) as GameObject;
                waypoints.RemoveAt(tempIndex);

                // Randomly select material hue/lum
                int matIndex = Random.Range(0, allMaterials.Count);
                searchArray[i + 1].GetComponent<Renderer>().material = allMaterials[matIndex];
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
