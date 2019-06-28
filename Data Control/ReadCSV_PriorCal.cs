using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReadCSV_PriorCal : MonoBehaviour
{
    public bool fileloaded = false;
    public Material targetMaterial;
    public Material dist1Material;
    public Material dist2Material;
    public List<Material> mainPeripheral = new List<Material>();

    List<string> listB1 = new List<string>();
    List<string> listC1 = new List<string>();
    List<string> listD1 = new List<string>();

    List<string> listB2 = new List<string>();
    List<string> listC2 = new List<string>();
    List<string> listD2 = new List<string>();
    
    string expPathColor;
    string expPathPeri;

    void Start ()
    {
        // access color and peripheral folders
        string filePathColor = string.Format("{0}/ColorProperties/", Application.dataPath);
        string filePathPeri = string.Format("{0}/PeripheralProperties/", Application.dataPath);

        // get all csv files in color and peripheral folders
        string[] fileArrayColor = Directory.GetFiles(filePathColor, "*csv");
        string[] fileArrayPeri = Directory.GetFiles(filePathPeri, "*csv");

        if (fileArrayColor.Length > 0)
        {
            expPathColor = string.Format("{0}/ColorProperties/ColorProperties_{1}.csv",
            Application.dataPath, fileArrayColor.Length);

            SetColors(expPathColor);
            fileloaded = true;
        }

        if (fileArrayPeri.Length > 0)
        {
            expPathPeri = string.Format("{0}/PeripheralProperties/PeripheralProperties_{1}.csv",
            Application.dataPath, fileArrayPeri.Length);

            SetPeripherals(expPathPeri);
        }
    }

    void SetColors(string filePath)
    {

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

        // Adjust Red Material
        int[] dist1Vals = { 0, 0, 0 };
        dist1Vals[0] = Convert.ToInt32(listB1[2]);
        dist1Vals[1] = Convert.ToInt32(listC1[2]);
        dist1Vals[2] = Convert.ToInt32(listD1[2]);
        dist1Material.color = new Color32((byte)dist1Vals[0], (byte)dist1Vals[1], (byte)dist1Vals[2], 255);

        // Adjust Green Material
        int[] dist2Vals = { 0, 0, 0 };
        dist2Vals[0] = Convert.ToInt32(listB1[3]);
        dist2Vals[1] = Convert.ToInt32(listC1[3]);
        dist2Vals[2] = Convert.ToInt32(listD1[3]);
        dist2Material.color = new Color32((byte)dist2Vals[0], (byte)dist2Vals[1], (byte)dist2Vals[2], 255);
    }

    void SetPeripherals(string filePath)
    {
        // Read CSV
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                //listA.Add(values[0]);   // column A
                listB2.Add(values[1]);   // column B
                listC2.Add(values[2]);   // column C
                listD2.Add(values[3]);   // column D
            }
        }

        // Adjust peripheral materials
        for (int i = 0; i < mainPeripheral.Count; i++)
        {
            int[] peripheralVals = { 0, 0, 0 };
            peripheralVals[0] = Convert.ToInt32(listB2[i + 1]);
            peripheralVals[1] = Convert.ToInt32(listC2[i + 1]);
            peripheralVals[2] = Convert.ToInt32(listD2[i + 1]);
            mainPeripheral[i].color = new Color32((byte)peripheralVals[0], (byte)peripheralVals[1], (byte)peripheralVals[2], 255);
        }
        //Debug.Log(mainPeripheral.Count + oppPeripheral.Count);
        
    }

}
