using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMaterial : MonoBehaviour {

    public Material targetColor;
    public Material oppositeColor;
    public List<GameObject> targetObjects = new List<GameObject>();
    public List<GameObject> oppObjects = new List<GameObject>();

	void Start ()
    {
		for (int i = 0; i < targetObjects.Count; i++)
        {
            targetObjects[i].GetComponent<Renderer>().material = targetColor;
        }

        for (int i = 0; i < oppObjects.Count; i++)
        {
            oppObjects[i].GetComponent<Renderer>().material = oppositeColor;
        }
    }
}
