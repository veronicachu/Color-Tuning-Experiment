using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class CalibrateAllColors : MonoBehaviour
{
    #region Variables
    public float lumStepSize;           // input number of luminance step size
    public float hueStepSize;           // input number of hue step size
    public float satStepSize;           // input number of saturation step size

    public float[] distAngle;

    private ImportLookup lookupRef;
    private IDictionary<int, byte> bluelookup;
    private IDictionary<int, byte> redlookup;
    private IDictionary<int, byte> greenlookup;

    private DKLConversion m_dklConversion;
    public List<float> deg = new List<float>();    // degree away from baseline color

    public GameObject targetObject;                 // input target blue gameobject to be changed
    private Material targetMaterial;
    public int targetRed, targetGreen, targetBlue;  // input starting blue's blue value (unfiltered)
    public float targetVectorLength, targetAzimuth, targetElevation;

    public GameObject dist1Object;                  // input target red gameobject to be changed
    private Material dist1Material;
    public int dist1Red, dist1Green, dist1Blue;     // input starting dist's blue value (unfiltered)
    //[HideInInspector]
    public float dist1VectorLength, dist1Azimuth, dist1Elevation;

    public GameObject dist2Object;                  // input target green gameobject to be changed
    private Material dist2Material;
    public int dist2Red, dist2Green, dist2Blue;     // input starting green's blue value (unfiltered)
    //[HideInInspector]
    public float dist2VectorLength, dist2Azimuth, dist2Elevation;

    public List<GameObject> peripheralObjects = new List<GameObject>();         // input peripheral colors
    private Material[] peripheralMaterials = new Material[50];
    private float[] peripheralVectorLength = new float[50];
    private float[] peripheralAzimuth = new float[50];
    private float[] peripheralElevation = new float[50];

    private bool fileloaded = false;

    public int startCall;
    private int centCallNum;
    public int periCallNum = 1;

    private bool cenOn = true;
    private bool lumOn = true;
    private bool peri0On = false;

    private bool blueFlag = false;
    private bool redFlag = false;
    private bool greenFlag = false;

    private int counter = 0;
    private GameObject[] objects = new GameObject[2];

    public Text instructions;           // input instructions UI text object
    public GameObject errorText;        // input error gameobject

    private StringBuilder csvColor = new StringBuilder();
    private StringBuilder csvPeri = new StringBuilder();
    private string expPathColor;
    private string expPathPeri;
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

        // Access peripheral color gameobjects' materials
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralMaterials[i] = peripheralObjects[i].GetComponent<InputMaterial>().targetColor;
        }

        // Set blue, red, and green object's material colors by inputing the unfiltered   
        // starting values entered in the inspector to the lookup tables
        targetMaterial.color = new Color32(redlookup[targetRed], greenlookup[targetGreen], bluelookup[targetBlue], 255);
        dist1Material.color = new Color32(redlookup[dist1Red], greenlookup[dist1Green], bluelookup[dist1Blue], 255);
        dist2Material.color = new Color32(redlookup[dist2Red], greenlookup[dist2Green], bluelookup[dist2Blue], 255);

        // Get starting DKL values for blue, red, and green object's material
        GetStartingDKL();

        // Set scene by hiding peripheral gameobjects
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralObjects[i].SetActive(false);
        }

        // Setup for save file containing RGB values for each blue, red, and green 
        // objects' material colors
        // Create "ColorProperties" folder in game files if non-existant
        Directory.CreateDirectory(Application.dataPath + "/ColorProperties");
        // Create "PeripheralProperties" folder in game files if non-existant
        Directory.CreateDirectory(Application.dataPath + "/PeripheralProperties");

        // Collect all of the previous csv files in the "ColorProperties" and count them 
        // to determine number of current file
        string filePathColor = string.Format("{0}/ColorProperties/", Application.dataPath);
        string[] fileArrayColor = Directory.GetFiles(filePathColor, "*csv");
        int numColor = fileArrayColor.Length + 1;

        // Collect all of the previous csv files in the "PeripheralProperties" and count them 
        // to determine number of current file
        string filePathPeri = string.Format("{0}/PeripheralProperties/", Application.dataPath);
        string[] fileArrayPeri = Directory.GetFiles(filePathPeri, "*csv");
        int numPeri = fileArrayPeri.Length + 1;

        // Create current save file's name and path
        expPathColor = string.Format("{0}/ColorProperties/ColorProperties_{1}.csv",
        Application.dataPath, numColor);

        // Create current save file's name and path
        expPathPeri = string.Format("{0}/PeripheralProperties/PeripheralProperties_{1}.csv",
        Application.dataPath, numPeri);
        
        centCallNum = startCall;
    }

    void Update()
    {
        #region Central Colors Call Numbers
        // Each call number section does the following:
        // >> Set instructions in the instructions UI text object
        // >> Calls the method that changes the specific DKL color property 
        //      (hue, luminance, saturation) of the active color's material

        if (cenOn && centCallNum == 0)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(false);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 1 Hue";
            targetAzimuth = ChangeHue(targetVectorLength, targetAzimuth, targetElevation, targetMaterial);
        }
        if (cenOn && centCallNum == 1)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(false);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 1 Luminance";
            targetVectorLength = ChangeLuminance(targetVectorLength, targetAzimuth, targetElevation, targetMaterial);
            dist1Azimuth = targetAzimuth + distAngle[0];
            dist2Azimuth = targetAzimuth + distAngle[1];
        }
        if (cenOn && centCallNum == 2)
        {
            targetObject.SetActive(false);
            dist1Object.SetActive(true);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 2 Luminance";
            dist1VectorLength = ChangeLuminance(dist1VectorLength, dist1Azimuth, dist1Elevation, dist1Material);
        }
        if (cenOn && centCallNum == 3)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(true);
            dist2Object.SetActive(false);
            instructions.text = "Adjust Color 2 Saturation";
            dist1Elevation = ChangeSaturation(dist1VectorLength, dist1Azimuth, dist1Elevation, dist1Material);
        }
        if (cenOn && centCallNum == 4)
        {
            targetObject.SetActive(false);
            dist1Object.SetActive(false);
            dist2Object.SetActive(true);
            instructions.text = "Adjust Color 3 Luminance";
            dist2VectorLength = ChangeLuminance(dist2VectorLength, dist2Azimuth, dist2Elevation, dist2Material);
        }
        if (cenOn && centCallNum == 5)
        {
            targetObject.SetActive(true);
            dist1Object.SetActive(false);
            dist2Object.SetActive(true);
            instructions.text = "Adjust Color 3 Saturation";
            dist2Elevation = ChangeSaturation(dist2VectorLength, dist2Azimuth, dist2Elevation, dist2Material);
        }
        if (cenOn && centCallNum == 6)
        {
            instructions.text = "Confirm Colors";
            targetObject.SetActive(true);
            dist1Object.SetActive(true);
            dist2Object.SetActive(true);
        }
        #endregion

        #region Navigation through the different central color calibration stages
        if (Input.GetKeyDown(KeyCode.RightArrow))
            centCallNum = centCallNum + 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            centCallNum = centCallNum - 1;

        // Make sure user doesn't break program by going too high or too low
        if (cenOn && centCallNum < startCall)
            centCallNum = startCall;
        if (cenOn && centCallNum > 6) // 6 parts: targ lum, dist1 lum, dist1 sat, dist2 lum, dist2 sat, central confirm
            centCallNum = 6;
        if (centCallNum == 6 && Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            cenOn = false;
            targetObject.SetActive(false);
            dist1Object.SetActive(false);
            dist2Object.SetActive(false);
            
            SetPeripheralColors();
        }
        if (Input.GetKeyDown(KeyCode.Q))
            cenOn = true;
        
        #endregion

        #region Peripheral Colors Call Numbers
        // Each call number section does the following:
        // >> Calls the method that changes the specific DKL color property 
        //      (luminance, saturation) of the active color's material
        // >> Set instructions in the instructions UI text object

        // Luminance Call Numbers
        if (!cenOn && periCallNum < peripheralObjects.Count && lumOn && !peri0On)
        {
            // hide target color object
            targetObject.SetActive(false);

            for (int i = 1; i < peripheralObjects.Count; i++)
            {
                peripheralObjects[i].SetActive(false);
            }
            peripheralObjects[periCallNum].SetActive(true);

            instructions.text = "Adjust Peripheral #" + (periCallNum + 1) + " Luminance";   // set instructions
            peripheralVectorLength[periCallNum] = ChangeLuminance(peripheralVectorLength[periCallNum], peripheralAzimuth[periCallNum], peripheralElevation[periCallNum], peripheralMaterials[periCallNum]);
        }

        // Saturation Call Numbers
        if (!cenOn && periCallNum > peripheralObjects.Count && periCallNum < peripheralObjects.Count * 2 && !lumOn)
        {
            // show target color object for saturation comparison
            targetObject.SetActive(true);

            int temp = periCallNum - peripheralObjects.Count;
            for (int i = 1; i < peripheralObjects.Count; i++)
            {
                peripheralObjects[i].SetActive(false);
            }
            peripheralObjects[temp].SetActive(true);

            instructions.text = "Adjust Peripheral #" + (temp + 1) + " Saturation";   // set instructions
            peripheralElevation[temp] = ChangeSaturation(peripheralVectorLength[temp], peripheralAzimuth[temp], peripheralElevation[temp], peripheralMaterials[temp]);
        }
        #endregion

        #region Navigate through the different calibration stages
        // Make sure peripheral calibration objects hidden during central calibration
        if (cenOn)
        {
            for (int i = 1; i < peripheralObjects.Count; i++)
            {
                peripheralObjects[i].SetActive(false);
            }
        }

        // Switch between peripheral 0 and current peripheral
        if (!cenOn && Input.GetKeyDown(KeyCode.UpArrow) && !peri0On && periCallNum < peripheralObjects.Count)
        {
            //peripheralObjects[0].SetActive(true);
            //peripheralObjects[periCallNum].SetActive(false);
            StartFlicker(peripheralObjects[periCallNum], peripheralObjects[0]);

            peri0On = true;
        }
        else if (!cenOn && Input.GetKeyDown(KeyCode.DownArrow) && peri0On && periCallNum < peripheralObjects.Count)
        {
            //peripheralObjects[0].SetActive(false);
            //peripheralObjects[periCallNum].SetActive(true);
            StopFlicker();

            peri0On = false;
        }

        // Navigate through the different peripheral calibration stages
        if (!cenOn && !peri0On && Input.GetKeyDown(KeyCode.RightArrow))
            periCallNum = periCallNum + 1;
        else if (!cenOn && !peri0On && Input.GetKeyDown(KeyCode.LeftArrow))
            periCallNum = periCallNum - 1;

        // Toggle luminance and saturation calibration
        if (!cenOn && Input.GetKeyDown(KeyCode.Space) && lumOn)
        {
            periCallNum = periCallNum + peripheralObjects.Count;
            lumOn = false;
        }
        else if (!cenOn && Input.GetKeyDown(KeyCode.Space) && !lumOn)
        {
            periCallNum = periCallNum - peripheralObjects.Count;
            lumOn = true;
        }

        // Make sure user doesn't break program by going too high or too low
        if (!cenOn && periCallNum < 1)  // no lower than 0
            periCallNum = 1;
        if (!cenOn && periCallNum > peripheralObjects.Count * 2 - 1)    // no more than (numPeri*2)-1
            periCallNum = peripheralObjects.Count * 2 - 1;
        if (!cenOn && periCallNum > peripheralObjects.Count - 1 && lumOn) // no more than numPeri when lum phase
            periCallNum = peripheralObjects.Count - 1;
        if (!cenOn && periCallNum < peripheralObjects.Count+1 && !lumOn) // no less than numPeri when sat phase
            periCallNum = peripheralObjects.Count;
        #endregion

        // Hit the 'Escape' key to activate method that writes and saves
        // the color materials' RGB values in a csv file
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            WriteCentralProperties();
            WritePeripheralProperties();
        }
    }

    // This method calculates the respective DKL values for each target color material
    void GetStartingDKL()
    {
        // Convert RGB to DKL-hue&sat (for target color)
        float[] targetDKLProperties = m_dklConversion.RGBtoDKL(targetRed, targetGreen, targetBlue, 0);
        targetVectorLength = 1.0f;      // starting DKL saturation value
        //targetAzimuth = 0f;              // starting DKL hue value
        targetElevation =20f;          // starting DKL luminance value

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
    
    // This method changes the peripheral colors once the central colors are set
    void SetPeripheralColors()
    {
        // Set starting DKL hue for each peripheral color by inputing baseline blue0's DKL hue 
        // and increase by the appropriate degrees
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralAzimuth[i] = StartingColors(peripheralMaterials[i], deg[i], targetAzimuth);
        }

        // Set starting DKL saturation and DKL luminance for each peripheral color to equal
        // baseline color's respective values
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralVectorLength[i] = targetVectorLength;
            peripheralElevation[i] = targetElevation - 5f;
        }
    }

    // This method changes the DKL hue value of a target color material by the input degree value and
    // implements the resulting RGB values to the target color material
    float StartingColors(Material blueMaterial, float deg, float startHue)
    {
        // INPUT: 1) target color material
        //        2) degree value to change hue
        //        3) baseline blue's DKL hue value

        // OUTPUT: target color's DKL hue value

        // Added starting DKL hue value with the degree change value
        float endHue = startHue + (deg / 1);

        // Convert DKL values to RGB values
        int[] rgb = m_dklConversion.DKLtoRGB(targetVectorLength, endHue, 15f, 0);
        Debug.Log("r=" + rgb[0] + "g=" + rgb[1] + "b=" + rgb[2]);

        // Implement color change to target color material
        blueMaterial.color = new Color32(redlookup[rgb[0]], greenlookup[rgb[1]], bluelookup[rgb[2]], 255);

        return endHue;
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
    void WriteCentralProperties()
    {
        // Write 1st line of csv file consisting of variable names
        string header = string.Format("{0},{1},{2},{3}",
            "Color", "Output R", "Output G", "Output B");
        csvColor.AppendLine(header);

        // Write 2nd line of csv file consisting of target RGB values
        Color32 endTarget = targetMaterial.color;
        string targetLine = string.Format("Target, {0}, {1}, {2}",
            endTarget.r, endTarget.g, endTarget.b);
        csvColor.AppendLine(targetLine);

        // Write 3rd line of csv file consisting of dist1 RGB values
        Color32 endDist1 = dist1Material.color;
        string dist1Line = string.Format("Dist 1, {0}, {1}, {2}",
            endDist1.r, endDist1.g, endDist1.b);
        csvColor.AppendLine(dist1Line);

        // Write 4th line of csv file consisting of dist2 RGB values
        Color32 endDist2 = dist2Material.color;
        string dist2Line = string.Format("Dist 2, {0}, {1}, {2}",
            endDist2.r, endDist2.g, endDist2.b);
        csvColor.AppendLine(dist2Line);

        // Write 5th live of csv file consisting of target DKL values
        string targetDKLLine = string.Format("Target DKL, {0}, {1}, {2}",
            targetVectorLength, targetAzimuth, targetElevation);
        csvColor.AppendLine(targetDKLLine);

        // Save writen csv file
        File.WriteAllText(expPathColor, csvColor.ToString());
    }

    void WritePeripheralProperties()
    {
        // Write 1st line of csv file consisting of variable names
        string header = string.Format("{0},{1},{2},{3}",
            "Color", "Output R", "Output G", "Output B");
        csvPeri.AppendLine(header);

        // Write lines of csv file for each peripheral color
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            Color32 endColor = peripheralMaterials[i].color;
            string colorLine1 = string.Format(peripheralMaterials[i].name + ", {0}, {1}, {2}",
                endColor.r, endColor.g, endColor.b);
            csvPeri.AppendLine(colorLine1);
        }

        // Save writen csv file
        File.WriteAllText(expPathPeri, csvPeri.ToString());
    }



    // This method starts the flicker at the given frequency rate
    public void StartFlicker(GameObject a, GameObject b)
    {
        objects[0] = a;
        objects[1] = b;

        float Frequency = 5f;
        float freq = (1.0f / (Frequency * 2f));
        InvokeRepeating("CycleColors", freq, freq);
    }

    // This method stops the flicker
    public void StopFlicker()
    {
        CancelInvoke("CycleColors");
        objects[0].SetActive(true);
        objects[1].SetActive(false);
    }

    // This controls cycling between the two colors
    void CycleColors()
    {
        objects[counter].SetActive(false);
        counter = ++counter % objects.Length;
        objects[counter].SetActive(true);
    }
}
