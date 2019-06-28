/*
 Backbone of the DKL peripheral stimulus calibration scene
 
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

public class CalibratePeripheral : MonoBehaviour
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

    private ReadCSV_target m_readCSVtarget;
    private DKLConversion m_dklConversion;
    
    public List<int> deg = new List<int>();    // degree away from baseline color

    public GameObject targetObject0;      // input baseline blue gameobject for reference (not changed)
    private Material targetMaterial0;
    private float targetVectorLength, targetAzimuth, targetElevation;
    private float red, green, blue;

    public List<GameObject> peripheralObjects = new List<GameObject>();         // input peripheral colors
    public List<GameObject> miniPeripheralObjects = new List<GameObject>();     // input reference peripheral color
    public List<GameObject> miniOppPeripheralObjects = new List<GameObject>();  // input reference opposite peripheral color

    private Material[] peripheralMaterials = new Material[50];
    private Material[] oppPeripheralMaterials = new Material[50];

    private float[] peripheralVectorLength = new float[50];
    private float[] peripheralAzimuth = new float[50];
    private float[] peripheralElevation = new float[50];
    private float[] oppPeripheralVectorLength = new float[50];
    private float[] oppPeripheralAzimuth = new float[50];
    private float[] oppPeripheralElevation = new float[50];

    private int callNum = 0;            // must be 0; used as input for lists later on
    private bool lumOn = true;

    public Text instructions;           // input instructions UI text object
    public GameObject errorText;        // input error gameobject
    public GameObject numContainer;     // input gameobject containing number references

    private StringBuilder csv = new StringBuilder();
    private string expPath;
    #endregion

    void Start()
    {
        // Create reference to ImportLookup.cs script -- reads lookup table csv files
        // ImportLookup.cs is placed on Main Camera in scene
        lookupRef = GameObject.Find("Main Camera").GetComponent<ImportLookup>();

        // Create reference to ReadCSV_target.cs script -- get the azimuth of the target
        m_readCSVtarget = GameObject.Find("Main Camera").GetComponent<ReadCSV_target>();

        // Create reference to DKLConversion.cs script -- converts between RGB and DKL
        // DKLConversion.cs is placed on same gameobject as current script
        m_dklConversion = this.GetComponent<DKLConversion>();

        // Access lookup tables using ImportLookup.cs script for each Red, Green, and Blue
        bluelookup = lookupRef.bluedict;
        redlookup = lookupRef.reddict;
        greenlookup = lookupRef.greendict;

        // Access baseline blue (blue0), peripheral color gameobjects' materials, 
        // and opposite peripheral color gameobjects' materials
        targetMaterial0 = targetObject0.GetComponent<Renderer>().material;
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralMaterials[i] = peripheralObjects[i].GetComponent<InputMaterial>().targetColor;
            oppPeripheralMaterials[i] = peripheralObjects[i].GetComponent<InputMaterial>().oppositeColor;
        }

        // Get RGB values of baseline color's material color
        Color32 targetBlue = targetMaterial0.color;
        red = targetBlue.r;
        green = targetBlue.g;
        blue = targetBlue.b;

        // Get DKL values for baseline color's material color
        targetVectorLength = 1f;// set starting luminance level
        targetAzimuth = m_readCSVtarget.targetAzimuth;
        targetElevation = 15f;  // set starting saturation level

        // Set starting DKL hue for each peripheral color by inputing baseline blue0's DKL hue 
        // and increase by the appropriate degrees
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralAzimuth[i] = StartingColors(peripheralMaterials[i], deg[i], targetAzimuth);
            oppPeripheralAzimuth[i] = StartingColors(oppPeripheralMaterials[i], deg[i] + 180f, targetAzimuth);
        }

        // Set starting DKL saturation and DKL luminance for each peripheral color to equal
        // baseline color's respective values
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            peripheralVectorLength[i] = targetVectorLength;
            peripheralElevation[i] = targetElevation;

            oppPeripheralVectorLength[i] = targetVectorLength;
            oppPeripheralElevation[i] = targetElevation;
        }

        // Set scene by activating blue1 gameobject and hiding other gameobjects
        targetObject0.SetActive(false);
        peripheralObjects[0].SetActive(true);

        for (int i = 1; i < peripheralObjects.Count; i++)
        {
            peripheralObjects[i].SetActive(false);
        }

        // Setup for save file containing RGB values for each blue1, blue2, blue3, and blue4 
        // objects' material colors
        // Create "PeripheralProperties" folder in game files if non-existant
        Directory.CreateDirectory(Application.dataPath + "/PeripheralProperties");

        // Collect all of the previous csv files in the "PeripheralProperties" and count them 
        // to determine number of current file
        string filePath = string.Format("{0}/PeripheralProperties/", Application.dataPath);
        string[] fileArray = Directory.GetFiles(filePath, "*csv");
        int num = fileArray.Length + 1;

        // Create current save file's name and path
        expPath = string.Format("{0}/PeripheralProperties/PeripheralProperties_{1}.csv",
        Application.dataPath, num);

        nextSceneNum = SceneManager.GetActiveScene().buildIndex + 1;
    }

    void Update()
    {
        // Each call number section does the following:
        // >> Calls the method that changes the specific DKL color property 
        //      (luminance, saturation) of the active color's material
        // >> Set instructions in the instructions UI text object
        #region Luminance Call Numbers
        if (callNum < peripheralObjects.Count && lumOn)
        {
            // hide target color object
            targetObject0.SetActive(false);

            for (int i = 0; i < peripheralObjects.Count; i++)
            {
                peripheralObjects[i].SetActive(false);
            }
            peripheralObjects[callNum].SetActive(true);

            instructions.text = "Adjust Peripheral #" + (callNum+1) + " Luminance";   // set instructions
            peripheralVectorLength[callNum] = ChangeLuminance(peripheralVectorLength[callNum], peripheralAzimuth[callNum], peripheralElevation[callNum], peripheralMaterials[callNum]);
            oppPeripheralVectorLength[callNum] = ChangeLuminance(oppPeripheralVectorLength[callNum], oppPeripheralAzimuth[callNum], oppPeripheralElevation[callNum], oppPeripheralMaterials[callNum]);
        }

        #endregion

        #region Saturation Call Numbers
        if (callNum >= peripheralObjects.Count && callNum < peripheralObjects.Count*2 && !lumOn)
        {
            // show target color object for saturation comparison
            targetObject0.SetActive(true);

            int temp = callNum - peripheralObjects.Count;
            for (int i = 0; i < peripheralObjects.Count; i++)
            {
                peripheralObjects[i].SetActive(false);
            }
            peripheralObjects[temp].SetActive(true);

            instructions.text = "Adjust Peripheral #" + (temp+1) + " Saturation";   // set instructions
            peripheralElevation[temp] = ChangeSaturation(peripheralVectorLength[temp], peripheralAzimuth[temp], peripheralElevation[temp], peripheralMaterials[temp]);
            oppPeripheralElevation[temp] = ChangeSaturation(oppPeripheralVectorLength[temp], oppPeripheralAzimuth[temp], oppPeripheralElevation[temp], oppPeripheralMaterials[temp]);
        }
        #endregion
        
        // Navigate through the different calibration stages
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            callNum = callNum + 1;

            // Skip luminance adjustment for peripheral with 0 deg offset
            if (callNum == 0 || callNum == peripheralObjects.Count)
                callNum = callNum + 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            callNum = callNum - 1;

            // Skip saturation adjustment for peripheral with 0 deg offset
            if (callNum == 0 || callNum == peripheralObjects.Count)
                callNum = callNum - 1;
        }

        if (Input.GetKeyDown(KeyCode.Space) && lumOn)
            lumOn = false;
        else if (Input.GetKeyDown(KeyCode.Space) && !lumOn)
            lumOn = true;

        // Make sure user doesn't break program by going too high or too low
        if (callNum < 0)
            callNum = 0;
        if (callNum > peripheralObjects.Count*2)
            callNum = peripheralObjects.Count*2 - 1;
        if (callNum > peripheralObjects.Count && lumOn)
            callNum = peripheralObjects.Count - 1;
        if (callNum < peripheralObjects.Count && !lumOn)
            callNum = peripheralObjects.Count;

        // Hit the 'Escape' key to activate method that writes and saves
        // the color materials' RGB values in a csv file
        if (Input.GetKeyDown(KeyCode.Return)|| Input.GetKeyDown(KeyCode.KeypadEnter))
            WriteColorProperties();
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
        int[] rgb = m_dklConversion.DKLtoRGB(targetVectorLength, endHue, targetElevation, 0);
        
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
        //INPUTS: 1) DKL saturation value of target color material, 
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


    // This method collects the RGB values of each of the color materials to write
    // and save them in a csv file
    void WriteColorProperties()
    {
        // Write 1st line of csv file consisting of variable names
        string header = string.Format("{0},{1},{2},{3}",
            "Color", "Output R", "Output G", "Output B");
        csv.AppendLine(header);

        // Write lines of csv file for each peripheral color
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            Color32 endColor = peripheralMaterials[i].color;
            string colorLine1 = string.Format(peripheralMaterials[i].name + ", {0}, {1}, {2}",
                endColor.r, endColor.g, endColor.b);
            csv.AppendLine(colorLine1);
        }

        // Write lines of csv file for each opposite peripheral color
        for (int i = 0; i < peripheralObjects.Count; i++)
        {
            Color32 endColor = oppPeripheralMaterials[i].color;
            string colorLine1 = string.Format(oppPeripheralMaterials[i].name + ", {0}, {1}, {2}",
                endColor.r, endColor.g, endColor.b);
            csv.AppendLine(colorLine1);
        }

        // Save writen csv file
        File.WriteAllText(expPath, csv.ToString());

        // Move on to the next game scene
        SceneManager.LoadScene(nextSceneNum);
    }


    void RGBtoLAB(Material targetMaterial, float deg)
    {
        // get target RGB values
        float R = (float)red / 255;
        float G = (float)green / 255;
        float B = (float)blue / 255;

        Debug.Log("R = " + R);
        Debug.Log("G = " + G);
        Debug.Log("B = " + B);

        #region Convert RGB to XYZ
        //// Inverse sRGB companding
        //if (R > 0.0405f)
        //    R = Mathf.Pow((R + 0.055f) / 1.055f, 2.4f);
        //else
        //    R = R / 12.92f;

        //if (G > 0.0405f)
        //    G = Mathf.Pow((G + 0.055f) / 1.055f, 2.4f);
        //else
        //    G = G / 12.92f;

        //if (B > 0.0405f)
        //    B = Mathf.Pow((B + 0.055f) / 1.055f, 2.4f);
        //else
        //    B = B / 12.92f;

        // [X Y Z] = M (sRGB D65) * [R G B]
        float X = R * 0.4124f + G * 0.3576f + B * 0.1805f;
        float Y = R * 0.2126f + G * 0.7151f + B * 0.0721f;
        float Z = R * 0.0193f + G * 0.1192f + B * 0.9505f;

        Debug.Log("X = " + X);
        Debug.Log("Y = " + Y);
        Debug.Log("Z = " + Z);
        #endregion

        #region Convert XYZ to CIELab
        // Reference White D65
        X = X / 94.811f;
        Y = Y / 100f;
        Z = Z / 107.304f;

        // fx
        if (X > 0.008856f)
            X = Mathf.Pow(X, (1 / 3f));
        else
            X = (903.3f * X + 16f) / 116f;

        // fy
        if (Y > 0.008856f)
            Y = Mathf.Pow(Y, (1 / 3f));
        else
            Y = (903.3f * Y + 16f) / 116f;

        // fz
        if (Z > 0.008856f)
            Z = Mathf.Pow(Z, (1 / 3f));
        else
            Z = (903.3f * Z + 16f) / 116f;

        float L = (116f * Y) - 16f;
        float a = 500f * (X - Y);
        float b = 200f * (Y - Z);

        Debug.Log("L = " + L);
        Debug.Log("a = " + a);
        Debug.Log("b = " + b);
        #endregion

        #region Apply angle change in CIELab space
        float r = Mathf.Sqrt(Mathf.Pow(a, 2f) + Mathf.Pow(b, 2f));
        float theta = Mathf.Atan2(b, a);
        deg = (Mathf.PI / 180) * deg; // convert rad to deg
        theta = theta + deg;
        Debug.Log("r = " + r);
        Debug.Log("theta = " + theta);

        a = Mathf.Cos(theta) * r;
        b = Mathf.Sin(theta) * r;
        #endregion

        #region Convert CIELab to XYZ
        Y = (L + 16f) / 116f; // fy
        X = (a / 500f) + Y;  // fx
        Z = Y - (b / 200f);  // fz

        // xr
        if (Mathf.Pow(X, 3f) > 0.008856f)
            X = Mathf.Pow(X, 3f);
        else
            X = (116f * X - 16f) / 903.3f;

        // yr
        if (Mathf.Pow(((L + 16f) / 116f), 3f) > 903.3f * 0.008856f)
            Y = Mathf.Pow(((L + 16f) / 116f), 3f);
        else
            Y = L / 903.3f;

        // zr
        if (Mathf.Pow(Z, 3f) > 0.008856f)
            Z = Mathf.Pow(Z, 3f);
        else
            Z = (116f * Z - 16f) / 903.3f;

        // Reference White D65
        X = X * 94.811f;
        Y = Y * 100f;
        Z = Z * 107.304f;

        Debug.Log("X = " + X);
        Debug.Log("Y = " + Y);
        Debug.Log("Z = " + Z);

        #endregion

        #region Convert XYZ to RGB
        // [X Y Z] = M (sRGB D65)^-1 * [X Y Z]
        R = X * 3.2408f + Y * -1.5375f + Z * -0.4988f;
        G = X * -0.9691f + Y * 1.8761f + Z * 0.0417f;
        B = X * 0.0557f + Y * -0.2041f + Z * 1.0570f;

        //// sRGB Companding
        //if (Mathf.Pow(1.055f * R, 1 / 2.4f) - 0.055f > 0.0031308)
        //    R = Mathf.Pow(1.055f * R, 1 / 2.4f) - 0.055f;
        //else
        //    R = 12.92f * R;

        //if (Mathf.Pow(1.055f * G, 1 / 2.4f) - 0.055f > 0.0031308)
        //    G = Mathf.Pow(1.055f * G, 1 / 2.4f) - 0.055f;
        //else
        //    G = 12.92f * G;

        //if (Mathf.Pow(1.055f * B, 1 / 2.4f) - 0.055f > 0.0031308)
        //    B = Mathf.Pow(1.055f * B, 1 / 2.4f) - 0.055f;
        //else
        //    B = 12.92f * B;

        Debug.Log("R = " + R);
        Debug.Log("G = " + G);
        Debug.Log("B = " + B);

        //R = R * 255;
        //G = G * 255;
        //B = B * 255;

        #endregion

        if (R < 0)
            R = 0;
        else
            R = Mathf.Round(R * 255);

        if (G < 0)
            G = 0;
        else
            G = Mathf.Round(G * 255);

        if (B < 0)
            B = 0;
        else
            B = Mathf.Round(B * 255);

        targetMaterial.color = new Color32(redlookup[(int)R], greenlookup[(int)G], bluelookup[(int)B], 255);

    }

}
