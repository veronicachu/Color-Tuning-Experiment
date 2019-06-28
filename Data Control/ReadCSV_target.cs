using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReadCSV_target : MonoBehaviour
{
    public Material targetMaterial;
    public float targetAzimuth;

    List<string> listB1 = new List<string>();
    List<string> listC1 = new List<string>();
    List<string> listD1 = new List<string>();

    void Awake ()
    {
        SetTarget();
	}
	
	void SetTarget()
    {
        // Find number of files to determine latest one
        string folderPath = string.Format("{0}/ColorProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(folderPath, "*csv");

        // Find the file path to stored data
        string filePath = string.Format("{0}/ColorProperties/ColorProperties_{1}.csv",
                             Application.dataPath, fileArray.Length);

        // Read CSV
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                //listA.Add(values[0]);   // column A
                listB1.Add(values[1]);   // column B
                listC1.Add(values[2]);   // column C
                listD1.Add(values[3]);   // column D
            }
        }

        // Adjust Blue Material
        int[] targetVals = { 0, 0, 0 };
        targetVals[0] = Convert.ToInt32(listB1[1]);
        targetVals[1] = Convert.ToInt32(listC1[1]);
        targetVals[2] = Convert.ToInt32(listD1[1]);
        targetMaterial.color = new Color32((byte)targetVals[0], (byte)targetVals[1], (byte)targetVals[2], 255);

        targetAzimuth = float.Parse(listC1[4]);
    }
}
