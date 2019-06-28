/* 
 Backbone of the DKL foveal stimulus calibration scene
 
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

public class CalibrateColors_Exp : MonoBehaviour
{
    #region Variables
    private int nextSceneNum;            // input number of next scene
    public float lumStepSize;           // input number of luminance step size
    public float hueStepSize;           // input number of hue step size
    public float satStepSize;           // input number of saturation step size

    private ImportLookup lookupRef;
    private IDictionary<int, byte> bluelookup;
    private IDictionary<int, byte> redlookup;
    private IDictionary<int, byte> greenlookup;

    private DKLConversion m_dklConversion;

    public GameObject targetObject;                 // input target blue gameobject to be changed
    private Material targetMaterial;
    public int targetRed, targetGreen, targetBlue;  // input starting blue's blue value (unfiltered)
    public float targetVectorLength = 0, targetAzimuth = 0, targetElevation = 0;

    public GameObject dist1Object;                  // input target red gameobject to be changed
    private Material dist1Material;
    public int dist1Red, dist1Green, dist1Blue;     // input starting dist's blue value (unfiltered)
    [HideInInspector]
    public float dist1VectorLength = 0, dist1Azimuth = 0, dist1Elevation = 0;

    public GameObject dist2Object;                  // input target green gameobject to be changed
    private Material dist2Material;
    public int dist2Red, dist2Green, dist2Blue;     // input starting green's blue value (unfiltered)
    [HideInInspector]
    public float dist2VectorLength = 0, dist2Azimuth = 0, dist2Elevation = 0;

    public int startCall;
    private int callNum;

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
        targetMaterial = targetObject.GetComponent<Renderer>().material;
        dist1Material = dist1Object.GetComponent<Renderer>().material;
        dist2Material = dist2Object.GetComponent<Renderer>().material;

        // Set blue, red, and green object's material colors by inputing the unfiltered   
        // starting values entered in the inspector to the lookup tables
        targetMaterial.color = new Color32(redlookup[targetRed], greenlookup[targetGreen], bluelookup[targetBlue], 255);
        dist1Material.color = new Color32(redlookup[dist1Red], greenlookup[dist1Green], bluelookup[dist1Blue], 255);
        dist2Material.color = new Color32(redlookup[dist2Red], greenlookup[dist2Green], bluelookup[dist2Blue], 255);

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

        nextSceneNum = SceneManager.GetActiveScene().buildIndex + 1;
        callNum = startCall;
    }

    void Update()
    {
        #region Call Numbers
        // Each call number section does the following:
        // >> Set instructions in the instructions UI text object
        // >> Calls the method that changes the specific DKL color property 
        //      (hue, luminance, saturation) of the active color's material

        if (callNum == 0)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(false);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 1 Hue";
            targetAzimuth = ChangeHue(targetVectorLength, targetAzimuth, targetElevation, targetMaterial);
        }
        if (callNum == 1)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(false);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 1 Luminance";
            targetVectorLength = ChangeLuminance(targetVectorLength, targetAzimuth, targetElevation, targetMaterial);
            dist1Azimuth = targetAzimuth + 90f;
            dist2Azimuth = targetAzimuth - 90f;
        }
        if (callNum == 2)
        {
            targetObject.SetActive(false);
            dist1Object.SetActive(true);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 2 Luminance";
            dist1VectorLength = ChangeLuminance(dist1VectorLength, dist1Azimuth, dist1Elevation, dist1Material);
        }
        if (callNum == 3)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(true);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 2 Saturation";
            dist1Elevation = ChangeSaturation(dist1VectorLength, dist1Azimuth, dist1Elevation, dist1Material);
        }
        if (callNum == 4)
        {
            targetObject.SetActive(false);
            dist1Object.SetActive(false);
            dist2Object.SetActive(true);
            instructions.text = "Adjust Color 3 Luminance";
            dist2VectorLength = ChangeLuminance(dist2VectorLength, dist2Azimuth, dist2Elevation, dist2Material);
        }
        if (callNum == 5)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(false);
            dist2Object.SetActive(true);
            instructions.text = "Adjust Color 3 Saturation";
            dist2Elevation = ChangeSaturation(dist2VectorLength, dist2Azimuth, dist2Elevation, dist2Material);
        }
        if (callNum == 6)
        {
            instructions.text = "Confirm Colors";
            targetObject.SetActive(true);
            dist1Object.SetActive(true);
            dist2Object.SetActive(true);
        }
        #endregion

        #region Navigation through the different calibration stages
        if (Input.GetKeyDown(KeyCode.RightArrow))
            callNum = callNum + 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            callNum = callNum - 1;

        // Make sure user doesn't break program by going too high or too low
        if (callNum < startCall)
            callNum = startCall;
        if (callNum > 6)
            callNum = 6;
        #endregion

        // Hit the 'Escape' key to activate method that writes and saves
        // the color materials' RGB values in a csv file
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            WriteColorProperties();
    }


    // This method calculates the respective DKL values for each target color material
    void GetStartingDKL()
    {
        // Convert RGB to DKL-hue&sat (for target color)
        float[] targetDKLProperties = m_dklConversion.RGBtoDKL(targetRed, targetGreen, targetBlue, 0);
        targetVectorLength = 1.0f;      // starting DKL saturation value
        //targetAzimuth = 0f;              // starting DKL hue value
        targetElevation = 15f;          // starting DKL luminance value

        // Convert RGB to DKL-hue&sat (for dist 1 color)
        float[] dist1DKLProperties = m_dklConversion.RGBtoDKL(dist1Red, dist1Green, dist1Blue, 0);
        dist1VectorLength = 1.0f;
        //dist1Azimuth = 90f;
        dist1Elevation = 15f;
        
        // Convert RGB to DKL-hue&sat (for dist 2 color)
        float[] dist2DKLProperties = m_dklConversion.RGBtoDKL(dist2Red, dist2Green, dist2Blue, 0);
        dist2VectorLength = 1.0f;
        //dist2Azimuth = 270f;
        dist2Elevation = 15f;
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

            // Implement color change to target color material
            colorMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);

            // Give error text when value too high
            if (rgb[0] > 255 || rgb[1] > 255 || rgb[2] > 255 || elevation > 30)
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
            //Debug.Log("R = " + rgb[0] + ", G = " + rgb[1] + ", B = " + rgb[2]);

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

        // Write 2nd line of csv file consisting of target RGB values
        Color32 endTarget = targetMaterial.color;
        string targetLine = string.Format("Target, {0}, {1}, {2}",
            endTarget.r, endTarget.g, endTarget.b);
        csv.AppendLine(targetLine);

        // Write 3rd line of csv file consisting of dist1 RGB values
        Color32 endDist1 = dist1Material.color;
        string dist1Line = string.Format("Dist 1, {0}, {1}, {2}",
            endDist1.r, endDist1.g, endDist1.b);
        csv.AppendLine(dist1Line);

        // Write 4th line of csv file consisting of dist2 RGB values
        Color32 endDist2 = dist2Material.color;
        string dist2Line = string.Format("Dist 2, {0}, {1}, {2}",
            endDist2.r, endDist2.g, endDist2.b);
        csv.AppendLine(dist2Line);

        // Write 5th live of csv file consisting of target DKL values
        string targetDKLLine = string.Format("Target DKL, {0}, {1}, {2}",
            targetVectorLength, targetAzimuth, targetElevation);
        csv.AppendLine(targetDKLLine);

        // Save writen csv file
        File.WriteAllText(expPath, csv.ToString());

        // Move on to the next game scene
        SceneManager.LoadScene(nextSceneNum);
    }

}
