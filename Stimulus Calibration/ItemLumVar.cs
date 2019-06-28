using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class ItemLumVar : MonoBehaviour
{
    private ImportLookup lookupRef;
    private IDictionary<int, byte> bluelookup;
    private IDictionary<int, byte> redlookup;
    private IDictionary<int, byte> greenlookup;

    DKLConversion m_dklConversion;
    CalibrateAllColors m_CalibrateColors;

    public float lumChange;
    public float satChange;
    
    public Material dist1LumUp, dist1LumDown, dist1SatUp, dist1SatDown;
    public Material dist2LumUp, dist2LumDown, dist2SatUp, dist2SatDown;
    
    private float dist1Vlen, dist1Azimuth, dist1Elevation;
    private float dist2Vlen, dist2Azimuth, dist2Elevation;
    
    private float dist1LumIncrease, dist1LumDecrease, dist1SatIncrease, dist1SatDecrease;
    private float dist2LumIncrease, dist2LumDecrease, dist2SatIncrease, dist2SatDecrease;

    private StringBuilder csv = new StringBuilder();
    private string expPath;

    void Start()
    {
        lookupRef = GameObject.Find("Main Camera").GetComponent<ImportLookup>();
        redlookup = lookupRef.reddict;
        greenlookup = lookupRef.greendict;
        bluelookup = lookupRef.bluedict;

        m_dklConversion = this.GetComponent<DKLConversion>();
        m_CalibrateColors = this.GetComponent<CalibrateAllColors>();

        // Create "ItemProperties" folder in game files if non-existant
        Directory.CreateDirectory(Application.dataPath + "/ItemProperties");

        // Collect all of the previous csv files in the "ColorProperties" and count them 
        // to determine number of current file
        string filePath = string.Format("{0}/ItemProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(filePath, "*csv");
        int num = fileArray.Length + 1;

        // Create current save file's name and path
        expPath = string.Format("{0}/ItemProperties/ItemProperties_{1}.csv",
        Application.dataPath, num);
    }

    void Update()
    {
        dist1Vlen = m_CalibrateColors.dist1VectorLength;
        dist1Azimuth = m_CalibrateColors.dist1Azimuth;
        dist1Elevation = m_CalibrateColors.dist1Elevation;

        dist2Vlen = m_CalibrateColors.dist2VectorLength;
        dist2Azimuth = m_CalibrateColors.dist2Azimuth;
        dist2Elevation = m_CalibrateColors.dist2Elevation;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeMaterial();
            WriteCSV();
        }
    }

    void ChangeMaterial()
    {
        // Change vlen
        // Get changed values for increase and decrease luminance change
        dist1LumIncrease = dist1Vlen + (dist1Vlen * lumChange);
        dist1LumDecrease = dist1Vlen - (dist1Vlen * lumChange);

        dist2LumIncrease = dist2Vlen + (dist2Vlen * lumChange);
        dist2LumDecrease = dist2Vlen - (dist2Vlen * lumChange);

        // Change elevation
        // Get changed values for increase and decrease saturation change
        dist1SatIncrease = dist1Elevation + (dist1Elevation * satChange);
        dist1SatDecrease = dist1Elevation - (dist1Elevation * satChange);

        dist2SatIncrease = dist2Elevation + (dist2Elevation * satChange);
        dist2SatDecrease = dist2Elevation - (dist2Elevation * satChange);

        // Get RGB value of luminance changes
        int[] dist1lumUpRGB = m_dklConversion.DKLtoRGB(dist1LumIncrease, dist1Azimuth, dist1Elevation, 0);
        int[] dist1lumDownRGB = m_dklConversion.DKLtoRGB(dist1LumDecrease, dist1Azimuth, dist1Elevation, 0);

        int[] dist2lumUpRGB = m_dklConversion.DKLtoRGB(dist2LumIncrease, dist2Azimuth, dist2Elevation, 0);
        int[] dist2lumDownRGB = m_dklConversion.DKLtoRGB(dist2LumDecrease, dist2Azimuth, dist2Elevation, 0);

        // Get RGB value of saturation changes
        int[] dist1satUpRGB = m_dklConversion.DKLtoRGB(dist1Vlen, dist1Azimuth, dist1SatIncrease, 0);
        int[] dist1satDownRGB = m_dklConversion.DKLtoRGB(dist1Vlen, dist1Azimuth, dist1SatDecrease, 0);

        int[] dist2satUpRGB = m_dklConversion.DKLtoRGB(dist2Vlen, dist2Azimuth, dist2SatIncrease, 0);
        int[] dist2satDownRGB = m_dklConversion.DKLtoRGB(dist2Vlen, dist2Azimuth, dist2SatDecrease, 0);

        // Implement changes to the materials
        dist1LumUp.color = new Color32(redlookup[dist1lumUpRGB[0]], greenlookup[dist1lumUpRGB[1]], bluelookup[dist1lumUpRGB[2]], 255);
        dist1LumDown.color = new Color32(redlookup[dist1lumDownRGB[0]], greenlookup[dist1lumDownRGB[1]], bluelookup[dist1lumDownRGB[2]], 255);

        dist2LumUp.color = new Color32(redlookup[dist2lumUpRGB[0]], greenlookup[dist2lumUpRGB[1]], bluelookup[dist2lumUpRGB[2]], 255);
        dist2LumDown.color = new Color32(redlookup[dist2lumDownRGB[0]], greenlookup[dist2lumDownRGB[1]], bluelookup[dist2lumDownRGB[2]], 255);

        dist1SatUp.color = new Color32(redlookup[dist1satUpRGB[0]], greenlookup[dist1satUpRGB[1]], bluelookup[dist1satUpRGB[2]], 255);
        dist1SatDown.color = new Color32(redlookup[dist1satDownRGB[0]], greenlookup[dist1satDownRGB[1]], bluelookup[dist1satDownRGB[2]], 255);

        dist2SatUp.color = new Color32(redlookup[dist2satUpRGB[0]], greenlookup[dist2satUpRGB[1]], bluelookup[dist2satUpRGB[2]], 255);
        dist2SatDown.color = new Color32(redlookup[dist2satDownRGB[0]], greenlookup[dist2satDownRGB[1]], bluelookup[dist2satDownRGB[2]], 255);
    }

    void WriteCSV()
    {
        // Write 1st line of csv file consisting of variable names
        string header = string.Format("{0},{1},{2},{3}",
            "Color", "Output R", "Output G", "Output B");
        csv.AppendLine(header);

        // Write 1st line of csv file consisting of dist1LumUp RGB values
        Color32 dist1LumInc = dist1LumUp.color;
        string dist1LumUpLine = string.Format("dist1LumUp, {0}, {1}, {2}",
            dist1LumInc.r, dist1LumInc.g, dist1LumInc.b);
        csv.AppendLine(dist1LumUpLine);

        // Write 2nd line of csv file consisting of dist1LumDown RGB values
        Color32 dist1LumDec = dist1LumDown.color;
        string dist1LumDownLine = string.Format("dist1LumDown, {0}, {1}, {2}",
            dist1LumDec.r, dist1LumDec.g, dist1LumDec.b);
        csv.AppendLine(dist1LumDownLine);

        // Write 3rd line of csv file consisting of dist2LumUp RGB values
        Color32 dist2LumInc = dist2LumUp.color;
        string dist2LumUpLine = string.Format("dist2LumUp, {0}, {1}, {2}",
            dist2LumInc.r, dist2LumInc.g, dist2LumInc.b);
        csv.AppendLine(dist2LumUpLine);

        // Write 4th line of csv file consisting of dist2LumDown RGB values
        Color32 dist2LumDec = dist2LumDown.color;
        string dist2LumDownLine = string.Format("dist2LumDown, {0}, {1}, {2}",
            dist2LumDec.r, dist2LumDec.g, dist2LumDec.b);
        csv.AppendLine(dist2LumDownLine);


        // Write 5th line of csv file consisting of dist1LumUp RGB values
        Color32 dist1SatInc = dist1SatUp.color;
        string dist1SatIncLine = string.Format("dist1SatUp, {0}, {1}, {2}",
            dist1SatInc.r, dist1SatInc.g, dist1SatInc.b);
        csv.AppendLine(dist1SatIncLine);

        // Write 6th line of csv file consisting of dist1LumDown RGB values
        Color32 dist1SatDec = dist1SatDown.color;
        string dist1SatDecLine = string.Format("dist1SatDown, {0}, {1}, {2}",
            dist1SatDec.r, dist1SatDec.g, dist1SatDec.b);
        csv.AppendLine(dist1SatDecLine);

        // Write 7th line of csv file consisting of dist2LumUp RGB values
        Color32 dist2SatInc = dist2SatUp.color;
        string dist2SatIncLine = string.Format("dist2SatUp, {0}, {1}, {2}",
            dist2SatInc.r, dist2SatInc.g, dist2SatInc.b);
        csv.AppendLine(dist2SatIncLine);

        // Write 8th line of csv file consisting of dist2LumDown RGB values
        Color32 dist2SatDec = dist2SatDown.color;
        string dist2SatDecLine = string.Format("dist2SatDown, {0}, {1}, {2}",
            dist2SatDec.r, dist2SatDec.g, dist2SatDec.b);
        csv.AppendLine(dist2SatDecLine);


        // Save writen csv file
        File.WriteAllText(expPath, csv.ToString());
    }
}
