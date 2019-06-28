using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class LocationData_Control : MonoBehaviour
{
    StringBuilder csv;
    private string expPath;

    private ExperimentManager_Control m_ExpManager;
    private ExpTrial_Control e_ExpTrial;

    private GameObject[] arrayItems;
    private List<string> arrayColor = new List<string>();
    private List<string> arrayOrient = new List<string>();
    private List<Vector3> arrayLocs = new List<Vector3>();

    public void NewFile()
    {
        m_ExpManager = this.GetComponent<ExperimentManager_Control>();
        e_ExpTrial = this.GetComponent<ExpTrial_Control>();
        csv = new StringBuilder();

        // Start new stream writer
        int trialnum = m_ExpManager.trialNumber;
        expPath = FileName(trialnum + 1);

        // Write first line with data information
        string newLine = string.Format("{0},{1},{2},{3}",
            "Color", "Orientation", "X Location", "Y Location");
        csv.AppendLine(newLine);
    }

    public static string FileName(int trial)
    {
        Directory.CreateDirectory(Application.dataPath + "/LocationData");

        // Creates the file path to store into the Data folder
        return string.Format("{0}/LocationData/Trial {1}_{2}.csv",
                             Application.dataPath, trial,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void WriteData()
    {
        // Get array item gameobjects and extract location, color, and orientation
        arrayItems = e_ExpTrial.arrayItems;
        arrayColor.Clear();
        arrayOrient.Clear();
        arrayLocs.Clear();
        for (int i = 0; i < arrayItems.Length; i++)
        {
            arrayColor.Add(arrayItems[i].GetComponent<Renderer>().material.name);
            arrayOrient.Add(arrayItems[i].name);
            arrayLocs.Add(arrayItems[i].transform.position);
        }

        // Convert information to string and store into csv
        for (int i = 0; i < arrayLocs.Count; i++)
        {
            string color = arrayColor[i].Remove(arrayColor[i].Length - 11);     // remove " (Instance)" at end

            string orient = arrayOrient[i].Remove(arrayOrient[i].Length - 7);   // remove "(Clone)" at end
            int findT = arrayOrient[i].IndexOf("T");
            orient = orient.Substring(findT + 1);

            string itemX = arrayLocs[i].x.ToString("F2");
            string itemY = arrayLocs[i].y.ToString("F2");

            // Writes a line for one item with location, color, and orientation data
            string newLine = string.Format("{0},{1},{2},{3}",
                color, orient, itemX, itemY);
            csv.AppendLine(newLine);
        }
        csv.AppendLine();
    }

    public void SaveData(Color32 targColor, string targOrientation)
    {
        csv.AppendLine("Target Color R, Target Color G, Target Color B");
        
        // Writes a line target color
        string newLine1 = string.Format("{0},{1},{2}",
            targColor.r, targColor.g, targColor.b);
        csv.AppendLine(newLine1);

        // Adjust target orientation name
        int findT = targOrientation.IndexOf("T");
        targOrientation = targOrientation.Substring(findT + 1);

        csv.AppendLine("Target Orientation");
        // Writes a line target data and selection data
        string newLine2 = string.Format("{0}", targOrientation);
        csv.AppendLine(newLine2);
        
        File.WriteAllText(expPath, csv.ToString());
    }
}
