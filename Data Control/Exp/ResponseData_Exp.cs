using UnityEngine;
using System.Text;
using System.IO;

public class ResponseData_Exp : MonoBehaviour
{
    StringBuilder csv = new StringBuilder();
    private string expPath;

    public GameObject mainFlickObject;
    public GameObject oppFlickObject;

    void Start()
    {
        // Start new stream writer
        expPath = FileName();
        Debug.Log(expPath);

        // Write first line with data information
        string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
            "Main Freq", "Opp Freq", "Main Color", "Opp Freq",
            "Target Color", "Target Orientation", "Num Targets", "Response");
        csv.AppendLine(newLine);
    }

    public static string FileName()
    {
        Directory.CreateDirectory(Application.dataPath + "/ResponseData");

        // Creates the file path to store into the Data folder
        return string.Format("{0}/ResponseData/ResponseData_{1}.csv",
                             Application.dataPath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void WriteData(Color32 targColor, string targOrientation, int targetApperances, int response)
    {
        // Collect color frequencies in the trial
        string mainFreq = mainFlickObject.GetComponent<FlickerControl>().Frequency.ToString();
        string oppFreq = oppFlickObject.GetComponent<FlickerControl>().Frequency.ToString();

        // Collect peripheral colors in the trial
        string mainPeripheral = mainFlickObject.GetComponent<Renderer>().material.ToString();
        int findUS1 = mainPeripheral.IndexOf("_");
        int findSpace1 = mainPeripheral.IndexOf(" ");
        mainPeripheral = mainPeripheral.Substring(findUS1+1, findSpace1 - findUS1);

        string oppPeripheral = oppFlickObject.GetComponent<Renderer>().material.ToString();
        int findUS2 = oppPeripheral.IndexOf("_");
        int findSpace2 = oppPeripheral.IndexOf(" ");
        oppPeripheral = oppPeripheral.Substring(findUS2+1, findSpace2 - findUS2);

        // Adjust target color name
        string targColorStr = "[" + targColor.r + "-" + targColor.g + "-" + targColor.b + "]";

        // Adjust target orientation name
        int findT = targOrientation.IndexOf("T");
        int find0 = targOrientation.IndexOf("0");
        targOrientation = targOrientation.Substring(findT + 1, find0 - findT);

        // Writes a line with target and selection data
        string newLine = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
            mainFreq, oppFreq, mainPeripheral, oppPeripheral,
            targColorStr, targOrientation, targetApperances, response);
        csv.AppendLine(newLine);
    }

    public void SaveData()
    {
        File.WriteAllText(expPath, csv.ToString());
    }
}
