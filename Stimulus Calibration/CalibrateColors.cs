/* 
 Backbone of the DKL foveal stimulus calibration scene
 
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

public class CalibrateColors : MonoBehaviour
{
    #region Variables
    public int nextSceneNum;            // input number of next scene
    public float lumStepSize;           // input number of luminance step size
    public float hueStepSize;           // input number of hue step size
    public float satStepSize;           // input number of saturation step size

    private ImportLookup lookupRef;
    private IDictionary<int, byte> bluelookup;
    private IDictionary<int, byte> redlookup;
    private IDictionary<int, byte> greenlookup;

    private DKLConversion m_dklConversion;

    public GameObject blueObject;       // input target blue gameobject to be changed
    private Material blueMaterial;
    public int blueRed;                 // input starting blue's red value (unfiltered)
    public int blueGreen;               // input starting blue's green value (unfiltered)
    public int blueBlue;                // input starting blue's blue value (unfiltered)
    public float blueVectorLength = 0, blueAzimuth = 0, blueElevation = 0;

    public GameObject redObject;        // input target red gameobject to be changed
    private Material redMaterial;
    public int redRed;                  // input starting red's red value (unfiltered)
    public int redGreen;                // input starting red's green value (unfiltered)
    public int redBlue;                 // input starting red's blue value (unfiltered)
    public float redVectorLength = 0, redAzimuth = 0, redElevation = 0;

    public GameObject greenObject;      // input target green gameobject to be changed
    private Material greenMaterial;
    public int greenRed;                // input starting green's red value (unfiltered)
    public int greenGreen;              // input starting green's green value (unfiltered)
    public int greenBlue;               // input starting green's blue value (unfiltered)
    public float greenVectorLength = 0, greenAzimuth = 0, greenElevation = 0;

    private char colorNum = 'b';
    private int callNum = 1;

    private bool blueFlag = false;
    private bool redFlag = false;
    private bool greenFlag = false;

    public Text instructions;           // input instructions UI text object
    public GameObject errorText;        // input error gameobject

    private StringBuilder csv = new StringBuilder();
    private string expPath;
    #endregion

    void Start()
    {
        // Create reference to ImportLookup.cs script -- reads lookup table csv files
        // ImportLookup.cs is placed on Main Camera in scene
        lookupRef = GameObject.Find("Main Camera").GetComponent<ImportLookup>();

        // Create reference to DKLConversion.cs script -- converts between RGB and DKL
        // DKLConversion.cs is placed on same gameobject as current script
        m_dklConversion = this.GetComponent<DKLConversion>();

        // Access lookup tables using ImportLookup.cs script for each Red, Green, and Blue
        redlookup = lookupRef.reddict;
        greenlookup = lookupRef.greendict;
        bluelookup = lookupRef.bluedict;

        // Access blue, red, and green gameobjects' materials
        blueMaterial = blueObject.GetComponent<Renderer>().material;
        redMaterial = redObject.GetComponent<Renderer>().material;
        greenMaterial = greenObject.GetComponent<Renderer>().material;

        // Set blue, red, and green object's material colors by inputing the unfiltered   
        // starting values entered in the inspector to the lookup tables
        blueMaterial.color = new Color32(redlookup[blueRed], greenlookup[blueGreen], bluelookup[blueBlue], 255);
        redMaterial.color = new Color32(redlookup[redRed], greenlookup[redGreen], bluelookup[redBlue], 255);
        greenMaterial.color = new Color32(redlookup[greenRed], greenlookup[greenGreen], bluelookup[greenBlue], 255);

        // Get starting DKL values for blue, red, and green object's material
        GetStartingDKL();

        // Setup for save file containing RGB values for each blue, red, and green 
        // objects' material colors
        // Create "ColorProperties" folder in game files if non-existant
        Directory.CreateDirectory(Application.dataPath + "/ColorProperties");

        // Collect all of the previous csv files in the "ColorProperties" and count them 
        // to determine number of current file
        string filePath = string.Format("{0}/ColorProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(filePath, "*csv");
        int num = fileArray.Length + 1;

        // Create current save file's name and path
        expPath = string.Format("{0}/ColorProperties/ColorProperties_{1}.csv",
        Application.dataPath, num);
    }

    void Update()
    {
        #region Select Color Gameobject
        // 'b' key = activate blue gameobject, hide red and green gameobjects
        // 'r' key = activate red gameobject, hide blue and green gameobjects
        // 'g' key = activate green gameobject, hide red and blue gameobjects

        // each input also internally sets values for variables colorNum and callNum
        // colorNum is used in the "Select DKL Color Property (Luminance, Hue, Saturation)" section below
        // callNum is used in the "Call Numbers" section below
        if (Input.GetKeyDown(KeyCode.B))
        {
            blueObject.SetActive(true);
            redObject.SetActive(false);
            greenObject.SetActive(false);

            colorNum = 'b';
            callNum = 1;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            blueObject.SetActive(false);
            redObject.SetActive(true);
            greenObject.SetActive(false);

            colorNum = 'r';
            callNum = 4;
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            blueObject.SetActive(false);
            redObject.SetActive(false);
            greenObject.SetActive(true);

            colorNum = 'g';
            callNum = 7;
        }
        #endregion

        #region Select DKL Color Property (Luminance, Hue, Saturation)
        // 'Alpha1' key = changes only Luminance property of active color
        // 'Alpha2' key = changes only Hue property of active color
        // 'Alpha3' key = changes only Saturation property of active color

        // each input also internally sets values for variable callNum
        // callNum is used in the "Call Numbers" section below
        if (colorNum == 'b')
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                callNum = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                callNum = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                callNum = 3;
        }
        else if (colorNum == 'r')
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                callNum = 4;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                callNum = 5;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                callNum = 6;
        }
        else if (colorNum == 'g')
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                callNum = 7;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                callNum = 8;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                callNum = 9;
        }
        #endregion

        #region Call Numbers
        // Each call number section does the following:
        // >> Set instructions in the instructions UI text object
        // >> Calls the method that changes the specific DKL color property 
        //      (hue, luminance, saturation) of the active color's material
        if (callNum == 0)
        {
            blueObject.SetActive(true);
            redObject.SetActive(false);
            greenObject.SetActive(false);
            instructions.text = "Adjust Hue to Blue";
            blueAzimuth = ChangeHue(blueVectorLength, blueAzimuth, blueElevation, blueMaterial);
        }
        if (callNum == 2)
        {
            blueObject.SetActive(true);
            redObject.SetActive(false);
            greenObject.SetActive(false);
            instructions.text = "Adjust Blue Luminance";
            blueVectorLength = ChangeLuminance(blueVectorLength, blueAzimuth, blueElevation, blueMaterial);
        }
        if (callNum == 3)
        {
            blueObject.SetActive(true);
            redObject.SetActive(true);
            greenObject.SetActive(false);
            instructions.text = "Adjust Blue Saturation";
            blueElevation = ChangeSaturation(blueVectorLength, blueAzimuth, blueElevation, blueMaterial);
        }

        if (callNum == 0)
        {
            blueObject.SetActive(false);
            redObject.SetActive(true);
            greenObject.SetActive(false);
            instructions.text = "Adjust Hue to Red";
            redAzimuth = ChangeHue(redVectorLength, redAzimuth, redElevation, redMaterial);
        }
        if (callNum == 1)
        {
            blueObject.SetActive(false);
            redObject.SetActive(true);
            greenObject.SetActive(false);
            instructions.text = "Adjust Red Luminance";
            redVectorLength = ChangeLuminance(redVectorLength, redAzimuth, redElevation, redMaterial);
        }
        if (callNum == 0)
        {
            blueObject.SetActive(true);
            redObject.SetActive(true);
            greenObject.SetActive(false);
            instructions.text = "Adjust Red Saturation";
            redElevation = ChangeSaturation(redVectorLength, redAzimuth, redElevation, redMaterial);
        }

        if (callNum == 0)
        {
            blueObject.SetActive(false);
            redObject.SetActive(false);
            greenObject.SetActive(true);
            instructions.text = "Adjust Hue to Green";
            greenAzimuth = ChangeHue(greenVectorLength, greenAzimuth, greenElevation, greenMaterial);
        }
        if (callNum == 4)
        {
            blueObject.SetActive(false);
            redObject.SetActive(false);
            greenObject.SetActive(true);
            instructions.text = "Adjust Green Luminance";
            greenVectorLength = ChangeLuminance(greenVectorLength, greenAzimuth, greenElevation, greenMaterial);
        }
        if (callNum == 5)
        {
            blueObject.SetActive(false);
            redObject.SetActive(true);
            greenObject.SetActive(true);
            instructions.text = "Adjust Green Saturation";
            greenElevation = ChangeSaturation(greenVectorLength, greenAzimuth, greenElevation, greenMaterial);
        }
        if (callNum == 6)
        {
            instructions.text = "Confirm Colors";
            blueObject.SetActive(true);
            redObject.SetActive(true);
            //greenObject.SetActive(true);
        }
        #endregion

        // Navigate through the different calibration stages
        if (Input.GetKeyDown(KeyCode.RightArrow))
            callNum = callNum + 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            callNum = callNum - 1;

        // Make sure user doesn't break program by going too high or too low
        if (callNum < 1)
            callNum = 1;
        if (callNum > 10)
            callNum = 10;

        // Hit the 'Escape' key to activate method that writes and saves
        // the color materials' RGB values in a csv file
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            WriteColorProperties();
    }


    // This method calculates the respective DKL values for each target color material
    void GetStartingDKL()
    {
        // Convert RGB to DKL-hue&sat (for blue)
        float[] blueDKLProperties = m_dklConversion.RGBtoDKL(blueRed, blueGreen, blueBlue, 0);
        blueVectorLength = 1.5f; // blueDKLProperties[0];      // starting blue DKL saturation value
        blueAzimuth = 180f; // blueDKLProperties[1];             // starting blue DKL hue value
        blueElevation = 20f; // 7f; // blueDKLProperties[2];       // starting blue DKL luminance value

        // Convert RGB to DKL-hue&sat (for red)
        float[] redDKLProperties = m_dklConversion.RGBtoDKL(redRed, redGreen, redBlue, 0);
        redVectorLength = 1.0f; // redDKLProperties[0];      // starting red DKL saturation value
        redAzimuth = 0f; // redDKLProperties[1];             // starting red DKL hue value
        redElevation = 20f; // 7f; // redDKLProperties[2];       // starting red DKL luminance value


        // Convert RGB to DKL-hue&sat (for green)
        float[] greenDKLProperties = m_dklConversion.RGBtoDKL(greenRed, greenGreen, greenBlue, 0);
        greenVectorLength = 1.2f; // greenDKLProperties[0];      // starting green DKL saturation value
        greenAzimuth = 270f; // greenDKLProperties[1];             // starting green DKL hue value
        greenElevation = 20f; // 7f; // greenDKLProperties[2];       // starting green DKL luminance value
    }


    // This method changes the DKL saturation value of a target color material and 
    // implements the resulting RGB values to the target color material
    float ChangeSaturation(float vectorlength, float azimuth, float elevation, Material colorMaterial)
    {
        // INPUTS: 1) initial DKL saturation value of target color material, 
        //         2) DKL hue value of target color material, 
        //         3) DKL luminance value of target color material, 
        //         4) target color material

        // OUTPUTS: end DKL saturation value of target color material

        // Upward scrollwheel input increases the DKL saturation value
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            // Increase DKL saturation by adding step size number 
            elevation = elevation + satStepSize;

            // Convert DKL values to RGB values
            int[] rgb = m_dklConversion.DKLtoRGB(vectorlength, azimuth, elevation, 0);
            //Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

            // Give error text when value too high
            if (rgb[0] > 255 || rgb[1] > 255 || rgb[2] > 255 || elevation > 27)
            {
                errorText.SetActive(true);
                errorText.GetComponent<Text>().text = "Value too high";

                // Return to original saturation value
                elevation = elevation - satStepSize;
            }
            else
            {
                errorText.SetActive(false);

                // Implement color change to target color material
                colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);
            }
        }

        // Downward scollwheel input decreases the DKL saturation value
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            // Decrease DKL saturation by subtracting step size number
            elevation = elevation - satStepSize;

            // Convert DKL values to RGB values
            int[] rgb = m_dklConversion.DKLtoRGB(vectorlength, azimuth, elevation, 0);
            //Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

            // Give error text when value too low
            if (rgb[0] < 0 || rgb[1] < 0 || rgb[2] < 0 || elevation < 0)
            {
                errorText.SetActive(true);
                errorText.GetComponent<Text>().text = "Value too low";

                // Return to original saturation value
                elevation = elevation + satStepSize;
            }
            else
            {
                errorText.SetActive(false);

                // Implement color change to target color material
                colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);
            }
        }

        return elevation;
    }


    // This method changes the DKL hue value of a target color material and 
    // implements the resulting RGB values to the target color material
    float ChangeHue(float vectorlength, float azimuth, float elevation, Material colorMaterial)
    {
        // INPUTS: 1) DKL saturation value of target color material, 
        //         2) initial DKL hue value of target color material, 
        //         3) DKL luminance value of target color material, 
        //         4) target color material

        // OUTPUTS: end DKL hue value of target color material

        // Upward scrollwheel input increases the DKL hue value
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            // Increase DKL hue by adding step size number
            azimuth = azimuth + hueStepSize;

            // Convert DKL values to RGB values
            int[] rgb = m_dklConversion.DKLtoRGB(vectorlength, azimuth, elevation, 0);
            //Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

            // Implement color change to target color material
            colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);
        }

        // Downward scrollwheel input decreases the DKL hue value
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            // Decrease DKL hue by subtracting step size number
            azimuth = azimuth - hueStepSize;

            // Convert DKL values to RGB values
            int[] rgb = m_dklConversion.DKLtoRGB(vectorlength, azimuth, elevation, 0);
            //Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

            // Implement color change to target color material
            colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);
        }

        return azimuth;
    }


    // This method changes the DKL luminance value of a target color material and 
    // implements the resulting RGB values to the target color material
    float ChangeLuminance(float vectorlength, float azimuth, float elevation, Material colorMaterial)
    {
        // INPUTS: 1) DKL saturation value of target color material, 
        //         2) DKL hue value of target color material, 
        //         3) initial DKL luminance value of target color material, 
        //         4) target color material

        // OUTPUTS: end DKL luminance value of target color material

        // Upward scrollwheel input increases the DKL luminance value
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            // Increase DKL luminance by adding step size number 
            vectorlength = vectorlength + lumStepSize;

            // Convert DKL values to RGB values
            int[] rgb = m_dklConversion.DKLtoRGB(vectorlength, azimuth, elevation, 0);
            Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

            // Give error text when value too high
            if (rgb[0] > 255 || rgb[1] > 255 || rgb[2] > 255)
            {
                errorText.SetActive(true);
                errorText.GetComponent<Text>().text = "Value too high";

                // Return to original luminance value
                vectorlength = vectorlength - lumStepSize;
            }
            else
            {
                errorText.SetActive(false);

                // Implement color change to target color material
                colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);
            }
        }

        // Downward scrollwheel input decreases the DKL luminance value
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            // Decrease DKL luminance by subtracting step size number
            vectorlength = vectorlength - lumStepSize;

            // Convert DKL values to RGB values
            int[] rgb = m_dklConversion.DKLtoRGB(vectorlength, azimuth, elevation, 0);
            //Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

            // Give error text when value too low
            if (rgb[0] < 0 || rgb[1] < 0 || rgb[2] < 0 || vectorlength < 0)
            {
                errorText.SetActive(true);
                errorText.GetComponent<Text>().text = "Value too low";

                // Return to original luminance value
                vectorlength = vectorlength + lumStepSize;
            }
            else
            {
                errorText.SetActive(false);

                // Implement color change to target color material
                colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);
            }
        }

        return vectorlength;
    }


    // This method uses the HSV color space to change the luminance of a target color 
    // material and implements the resulting RGB values to the target color material
    // (use if given up on DKL space)
    float[] ChangeLuminance2(int[] colorRGB, float[] colorHSV, Material colorMaterial)
    {
        if (colorMaterial.name[0] == 'B' && !blueFlag)
        {
            Color.RGBToHSV(new Color(colorRGB[0] / 255f, colorRGB[1] / 255f, colorRGB[2] / 255f), out colorHSV[0], out colorHSV[1], out colorHSV[2]);
            blueFlag = true;
        }
        else if (colorMaterial.name[0] == 'R' && !redFlag)
        {
            Color.RGBToHSV(new Color(colorRGB[0] / 255f, colorRGB[1] / 255f, colorRGB[2] / 255f), out colorHSV[0], out colorHSV[1], out colorHSV[2]);
            redFlag = true;
        }
        else if (colorMaterial.name[0] == 'G' && !greenFlag)
        {
            Color.RGBToHSV(new Color(colorRGB[0] / 255f, colorRGB[1] / 255f, colorRGB[2] / 255f), out colorHSV[0], out colorHSV[1], out colorHSV[2]);
            greenFlag = true;
        }


        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            colorHSV[2] = colorHSV[2] + 0.01f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            colorHSV[2] = colorHSV[2] - 0.01f;
        }

        // tell when value too high/low
        if (colorHSV[2] < -0)
            errorText.SetActive(true);
        else
            errorText.SetActive(false);

        Color32 newColor = Color.HSVToRGB(colorHSV[0], colorHSV[1], colorHSV[2]);
        colorMaterial.color = new Color32(redlookup[newColor.r], greenlookup[newColor.g], bluelookup[newColor.b], 255);

        return colorHSV;
    }


    // This method collects the RGB values of each of the color materials to write
    // and save them in a csv file
    void WriteColorProperties()
    {
        // Write 1st line of csv file consisting of variable names
        string header = string.Format("{0},{1},{2},{3}",
            "Color", "Output R", "Output G", "Output B");
        csv.AppendLine(header);

        // Write 2nd line of csv file consisting of blue RGB values
        Color32 endBlue = blueMaterial.color;
        string blueLine = string.Format("Blue, {0}, {1}, {2}",
            endBlue.r, endBlue.g, endBlue.b);
        csv.AppendLine(blueLine);

        // Write 3rd line of csv file consisting of red RGB values
        Color32 endRed = redMaterial.color;
        string redLine = string.Format("Red, {0}, {1}, {2}",
            endRed.r, endRed.g, endRed.b);
        csv.AppendLine(redLine);

        // Write 4th line of csv file consisting of green RGB values
        Color32 endGreen = greenMaterial.color;
        string greenLine = string.Format("Green, {0}, {1}, {2}",
            endGreen.r, endGreen.g, endGreen.b);
        csv.AppendLine(greenLine);

        // Save writen csv file
        File.WriteAllText(expPath, csv.ToString());

        // Move on to the next game scene
        SceneManager.LoadScene(nextSceneNum);
    }

}
