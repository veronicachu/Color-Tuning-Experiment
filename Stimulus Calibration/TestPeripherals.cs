using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class TestPeripherals : MonoBehaviour {

    private ImportLookup lookupRef;
    private IDictionary<int, byte> bluelookup;
    private IDictionary<int, byte> redlookup;
    private IDictionary<int, byte> greenlookup;

    DKLConversion m_dklConversion;
    CalibrateColors m_CalibrateColors;

    public float lumChange;

    public Material redLumUp, redLumDown;
    public Material greenLumUp, greenLumDown;
    public Material blueLumUp, blueLumDown;

    private float redVlen, redAzimuth, redElevation;
    private float greenVlen, greenAzimuth, greenElevation;
    private float blueVlen, blueAzimuth, blueElevation;

    private float redlumIncrease, redlumDecrease;
    private float greenlumIncrease, greenlumDecrease;
    private float bluelumIncrease, bluelumDecrease;

    private StringBuilder csv = new StringBuilder();
    private string expPath;

    void Start ()
    {
        lookupRef = GameObject.Find("Main Camera").GetComponent<ImportLookup>();
        redlookup = lookupRef.reddict;
        greenlookup = lookupRef.greendict;
        bluelookup = lookupRef.bluedict;

        m_dklConversion = this.GetComponent<DKLConversion>();
        m_CalibrateColors = this.GetComponent<CalibrateColors>();

        // Create "PeripheralProperties" folder in game files if non-existant
        Directory.CreateDirectory(Application.dataPath + "/PeripheralProperties");

        // Collect all of the previous csv files in the "ColorProperties" and count them 
        // to determine number of current file
        string filePath = string.Format("{0}/PeripheralProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(filePath, "*csv");
        int num = fileArray.Length + 1;

        // Create current save file's name and path
        expPath = string.Format("{0}/PeripheralProperties/PeripheralProperties_{1}.csv",
        Application.dataPath, num);
    }
	
	void Update ()
    {
        redVlen = m_CalibrateColors.redVectorLength;
        redAzimuth = m_CalibrateColors.redAzimuth;
        redElevation = m_CalibrateColors.redElevation;

        greenVlen = m_CalibrateColors.greenVectorLength;
        greenAzimuth = m_CalibrateColors.greenAzimuth;
        greenElevation = m_CalibrateColors.greenElevation;

        blueVlen = m_CalibrateColors.blueVectorLength;
        blueAzimuth = m_CalibrateColors.blueAzimuth;
        blueElevation = m_CalibrateColors.blueElevation;

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            ChangeMaterial();
            WriteCSV();
        }
	}

    void ChangeMaterial()
    {
        // Change vlen
        // Get changed values for increase and decrease luminance change
        redlumIncrease = redVlen; // + (redVlen * lumChange);
        redlumDecrease = redVlen - (redVlen * lumChange);

        greenlumIncrease = greenVlen; // + (greenVlen * lumChange);
        greenlumDecrease = greenVlen - (greenVlen * lumChange);

        bluelumIncrease = blueVlen; // + (blueVlen * lumChange);
        bluelumDecrease = blueVlen - (blueVlen * lumChange);

        // Get RGB value of luminance changes
        int[] redlumUpRGB = m_dklConversion.DKLtoRGB(redlumIncrease, redAzimuth, redElevation, 0);
        int[] redlumDownRGB = m_dklConversion.DKLtoRGB(redlumDecrease, redAzimuth, redElevation, 0);

        int[] greenlumUpRGB = m_dklConversion.DKLtoRGB(greenlumIncrease, greenAzimuth, greenElevation, 0);
        int[] greenlumDownRGB = m_dklConversion.DKLtoRGB(greenlumDecrease, greenAzimuth, greenElevation, 0);

        int[] bluelumUpRGB = m_dklConversion.DKLtoRGB(bluelumIncrease, blueAzimuth, blueElevation, 0);
        int[] bluelumDownRGB = m_dklConversion.DKLtoRGB(bluelumDecrease, blueAzimuth, blueElevation, 0);

        // Implement changes to the materials
        redLumUp.color = new Color32(redlookup[redlumUpRGB[0]], greenlookup[redlumUpRGB[1]], bluelookup[redlumUpRGB[2]], 255);
        redLumDown.color = new Color32(redlookup[redlumDownRGB[0]], greenlookup[redlumDownRGB[1]], bluelookup[redlumDownRGB[2]], 255);

        greenLumUp.color = new Color32(redlookup[greenlumUpRGB[0]], greenlookup[greenlumUpRGB[1]], bluelookup[greenlumUpRGB[2]], 255);
        greenLumDown.color = new Color32(redlookup[greenlumDownRGB[0]], greenlookup[greenlumDownRGB[1]], bluelookup[greenlumDownRGB[2]], 255);

        blueLumUp.color = new Color32(redlookup[bluelumUpRGB[0]], greenlookup[bluelumUpRGB[1]], bluelookup[bluelumUpRGB[2]], 255);
        blueLumDown.color = new Color32(redlookup[bluelumDownRGB[0]], greenlookup[bluelumDownRGB[1]], bluelookup[bluelumDownRGB[2]], 255);
        
    }

    void WriteCSV()
    {
        // Write 1st line of csv file consisting of variable names
        string header = string.Format("{0},{1},{2},{3}",
            "Color", "Output R", "Output G", "Output B");
        csv.AppendLine(header);

        // Write 2nd line of csv file consisting of red lum up RGB values
        Color32 redlumup = redLumUp.color;
        string redLumUpLine = string.Format("RedLumUp, {0}, {1}, {2}",
            redlumup.r, redlumup.g, redlumup.b);
        csv.AppendLine(redLumUpLine);

        // Write 3rd line of csv file consisting of red lum up RGB values
        Color32 redlumdown = redLumDown.color;
        string redLumDownLine = string.Format("RedLumDown, {0}, {1}, {2}",
            redlumdown.r, redlumdown.g, redlumdown.b);
        csv.AppendLine(redLumDownLine);

        // Write 4th line of csv file consisting of green RGB values
        Color32 greenlumup = greenLumUp.color;
        string greenLumUpLine = string.Format("GreenLumUp, {0}, {1}, {2}",
            greenlumup.r, greenlumup.g, greenlumup.b);
        csv.AppendLine(greenLumUpLine);

        // Write 5th line of csv file consisting of green RGB values3
        Color32 greenlumdown = greenLumDown.color;
        string greenLumDownLine = string.Format("GreenLumDown, {0}, {1}, {2}",
            greenlumdown.r, greenlumdown.g, greenlumdown.b);
        csv.AppendLine(greenLumDownLine);

        // Write 4th line of csv file consisting of green RGB values
        Color32 bluelumup = blueLumUp.color;
        string blueLumUpLine = string.Format("BlueLumUp, {0}, {1}, {2}",
            bluelumup.r, bluelumup.g, bluelumup.b);
        csv.AppendLine(blueLumUpLine);
        Debug.Log("blueup" + bluelumup.r + bluelumup.g + bluelumup.b);

        // Write 5th line of csv file consisting of green RGB values
        Color32 bluelumdown = blueLumDown.color;
        string blueLumDownLine = string.Format("BlueLumDown, {0}, {1}, {2}",
            bluelumdown.r, bluelumdown.g, bluelumdown.b);
        csv.AppendLine(blueLumDownLine);
        
        

        // Save writen csv file
        File.WriteAllText(expPath, csv.ToString());
    }
}
