using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImportLookup : MonoBehaviour {

    public IDictionary<int, byte> reddict = new Dictionary<int, byte>();
    public IDictionary<int, byte> greendict = new Dictionary<int, byte>();
    public IDictionary<int, byte> bluedict = new Dictionary<int, byte>();

    void Awake ()
    {
        // import lookup table csv files
        var redlookup = File.ReadAllLines(Application.dataPath + "/Lookup Tables/lookup_red.csv");
        var greenlookup = File.ReadAllLines(Application.dataPath + "/Lookup Tables/lookup_green.csv");
        var bluelookup = File.ReadAllLines(Application.dataPath + "/Lookup Tables/lookup_blue.csv");

        Debug.Log(Application.dataPath + "/Lookup Tables/lookup_red.csv");
        //Debug.Log("Raw: " + bluelookup[101]);
        //Debug.Log("Raw Before Comma: " + bluelookup[101].Split(',')[0]); // before comma
        //Debug.Log("Raw After Comma: " + bluelookup[101].Substring(bluelookup[101].IndexOf(',') + 1)); // after comma

        // store imported data in dictionary -- red
        for (int i = 1; i < redlookup.Length; i++)
        {
            int key = Int32.Parse(redlookup[i].Split(',')[0]);
            byte value = byte.Parse(redlookup[i].Substring(redlookup[i].IndexOf(',') + 1));

            reddict.Add(key,value);
        }

        // store imported data in dictionary -- green
        for (int i = 1; i < greenlookup.Length; i++)
        {
            int key = Int32.Parse(greenlookup[i].Split(',')[0]);
            byte value = byte.Parse(greenlookup[i].Substring(greenlookup[i].IndexOf(',') + 1));

            greendict.Add(key, value);
        }

        // store imported data in dictionary -- green
        for (int i = 1; i < bluelookup.Length; i++)
        {
            int key = Int32.Parse(bluelookup[i].Split(',')[0]);
            byte value = byte.Parse(bluelookup[i].Substring(bluelookup[i].IndexOf(',') + 1));

            bluedict.Add(key, value);
        }
        //Debug.Log("Dictionary: " + bluedict[100]);
    }
}
