using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ReadCSV_Exp : MonoBehaviour
{
    public Material targetMaterial;
    public Material dist1Material;
    public Material dist2Material;
    
    public Material dist1LumUpMaterial, dist1LumDownMaterial, dist1SatUpMaterial, dist1SatDownMaterial;
    public Material dist2LumUpMaterial, dist2LumDownMaterial, dist2SatUpMaterial, dist2SatDownMaterial;

    public List<Material> mainPeripheral = new List<Material>();

    List<string> listB1 = new List<string>();
    List<string> listC1 = new List<string>();
    List<string> listD1 = new List<string>();

    List<string> listB2 = new List<string>();
    List<string> listC2 = new List<string>();
    List<string> listD2 = new List<string>();

    List<string> listB3 = new List<string>();
    List<string> listC3 = new List<string>();
    List<string> listD3 = new List<string>();

    void Start()
    {
        SetColors();
        SetPeripherals();
        SetItemColors();
    }

    void SetColors()
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

    void SetPeripherals()
    {
        // Find number of files to determine latest one
        string folderPath = string.Format("{0}/PeripheralProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(folderPath, "*csv");

        // Find the file path to stored data
        string filePath = string.Format("{0}/PeripheralProperties/PeripheralProperties_{1}.csv",
                             Application.dataPath, fileArray.Length);

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

        // Adjust main peripheral materials
        for(int i = 0; i < mainPeripheral.Count; i++)
        {
            int[] peripheralVals = { 0, 0, 0 };
            peripheralVals[0] = Convert.ToInt32(listB2[i+1]);
            peripheralVals[1] = Convert.ToInt32(listC2[i+1]);
            peripheralVals[2] = Convert.ToInt32(listD2[i+1]);
            mainPeripheral[i].color = new Color32((byte)peripheralVals[0], (byte)peripheralVals[1], (byte)peripheralVals[2], 255);
        }
        //Debug.Log(mainPeripheral.Count + oppPeripheral.Count);
    }

    void SetItemColors()
    {
        // Find number of files to determine latest one
        string folderPath = string.Format("{0}/ItemProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(folderPath, "*csv");

        // Find the file path to stored data
        string filePath = string.Format("{0}/ItemProperties/ItemProperties_{1}.csv",
                             Application.dataPath, fileArray.Length);

        // Read CSV
        using (var reader = new StreamReader(filePath))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                //listA.Add(values[0]);   // column A
                listB3.Add(values[1]);   // column B
                listC3.Add(values[2]);   // column C
                listD3.Add(values[3]);   // column D
            }
        }

        // Adjust Dist1 Lum Up Material
        int[] dist1LumUpVals = { 0, 0, 0 };
        dist1LumUpVals[0] = Convert.ToInt32(listB3[1]);
        dist1LumUpVals[1] = Convert.ToInt32(listC3[1]);
        dist1LumUpVals[2] = Convert.ToInt32(listD3[1]);
        dist1LumUpMaterial.color = new Color32((byte)dist1LumUpVals[0], (byte)dist1LumUpVals[1], (byte)dist1LumUpVals[2], 255);

        // Adjust Dist1 Lum Down Material
        int[] dist1LumDownVals = { 0, 0, 0 };
        dist1LumDownVals[0] = Convert.ToInt32(listB3[2]);
        dist1LumDownVals[1] = Convert.ToInt32(listC3[2]);
        dist1LumDownVals[2] = Convert.ToInt32(listD3[2]);
        dist1LumDownMaterial.color = new Color32((byte)dist1LumDownVals[0], (byte)dist1LumDownVals[1], (byte)dist1LumDownVals[2], 255);
        
        // Adjust Dist2 Lum Up Material
        int[] dist2LumUpVals = { 0, 0, 0 };
        dist2LumUpVals[0] = Convert.ToInt32(listB3[3]);
        dist2LumUpVals[1] = Convert.ToInt32(listC3[3]);
        dist2LumUpVals[2] = Convert.ToInt32(listD3[3]);
        dist2LumUpMaterial.color = new Color32((byte)dist2LumUpVals[0], (byte)dist2LumUpVals[1], (byte)dist2LumUpVals[2], 255);

        // Adjust Dist2 Lum Down Material
        int[] dist2LumDownVals = { 0, 0, 0 };
        dist2LumDownVals[0] = Convert.ToInt32(listB3[4]);
        dist2LumDownVals[1] = Convert.ToInt32(listC3[4]);
        dist2LumDownVals[2] = Convert.ToInt32(listD3[4]);
        dist2LumDownMaterial.color = new Color32((byte)dist2LumDownVals[0], (byte)dist2LumDownVals[1], (byte)dist2LumDownVals[2], 255);


        // Adjust Dist1 Sat Up Material
        int[] dist1SatUpVals = { 0, 0, 0 };
        dist1SatUpVals[0] = Convert.ToInt32(listB3[5]);
        dist1SatUpVals[1] = Convert.ToInt32(listC3[5]);
        dist1SatUpVals[2] = Convert.ToInt32(listD3[5]);
        dist1SatUpMaterial.color = new Color32((byte)dist1SatUpVals[0], (byte)dist1SatUpVals[1], (byte)dist1SatUpVals[2], 255);

        // Adjust Dist1 Sat Down Material
        int[] dist1SatDownVals = { 0, 0, 0 };
        dist1SatDownVals[0] = Convert.ToInt32(listB3[6]);
        dist1SatDownVals[1] = Convert.ToInt32(listC3[6]);
        dist1SatDownVals[2] = Convert.ToInt32(listD3[6]);
        dist1SatDownMaterial.color = new Color32((byte)dist1SatDownVals[0], (byte)dist1SatDownVals[1], (byte)dist1SatDownVals[2], 255);

        // Adjust Dist2 Sat Up Material
        int[] dist2SatUpVals = { 0, 0, 0 };
        dist2SatUpVals[0] = Convert.ToInt32(listB3[7]);
        dist2SatUpVals[1] = Convert.ToInt32(listC3[7]);
        dist2SatUpVals[2] = Convert.ToInt32(listD3[7]);
        dist2SatUpMaterial.color = new Color32((byte)dist2SatUpVals[0], (byte)dist2SatUpVals[1], (byte)dist2SatUpVals[2], 255);

        // Adjust Dist2 Sat Down Material
        int[] dist2SatDownVals = { 0, 0, 0 };
        dist2SatDownVals[0] = Convert.ToInt32(listB3[8]);
        dist2SatDownVals[1] = Convert.ToInt32(listC3[8]);
        dist2SatDownVals[2] = Convert.ToInt32(listD3[8]);
        dist2SatDownMaterial.color = new Color32((byte)dist2SatDownVals[0], (byte)dist2SatDownVals[1], (byte)dist2SatDownVals[2], 255);
    }

}

