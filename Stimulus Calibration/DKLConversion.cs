/* 
Contains 2 methods:
RGBtoDKL --> used to convert RGB values to DKL values
DKLtoRGB --> used to convert DKL values to RGB values

References:
Kaiser, P. K., & Boynton, R. M. (1996). Human color vision.
Brainard, D. H. (1996). Cone contrast and opponent modulation color spaces. In Kaiser and Boynton, Human Color Vision.
*/

using UnityEngine;

public class DKLConversion : MonoBehaviour
{

    public float outRL, outRLM, outRS;

    public float[] RGBtoDKL(float R, float G, float B, int flag)
    {
        // INPUTS: 1) R value of stimulus color
        //         2) G value of stimulus color
        //         3) B value of stimulus color
        //         4) flag variable accepts only 0 or 1: 0 skips cone contrast step (luminance), 
        //              1 does cone contrast step (hue or saturation)

        // OUTPUTS: array of DKL spherical coordinates [1] saturation (vector length) with cone contrast step, 
        //      [2] hue angle (in degrees)  with cone contrast step, [3] achromatic angle (in degrees) with 
        //      cone contrast step, [4] saturation (vector length) without cone contrast step, [5] hue angle 
        //      (in degrees)  without cone contrast step, [6] achromatic angle (in degrees) without cone 
        //      contrast step
        
        #region RGB --> XYZ
        // TRANSFORM RGB VALUES TO XYZ VALUES
        // calculated using formula: XYZ = RGB * M
        // where,
        // >> M = matrix for converting RGB to XYZ for sRGB (found in sRGB Wiki page)
        float X = R * 0.4124f + G * 0.3576f + B * 0.1805f;
        float Y = R * 0.2126f + G * 0.7151f + B * 0.0721f;
        float Z = R * 0.0193f + G * 0.1192f + B * 0.9505f;

        //Debug.Log("Pre X = " + X);
        //Debug.Log("Pre Y = " + Y);
        //Debug.Log("Pre Z = " + Z);
        #endregion

        #region XYZ --> Cone Stimulants (Cone Excitation Coordinates)
        // CALCULATE CONE EXCITATION COORDINANTES
        // calculated using formula: P = XYZ_stimulus * M
        // where,
        // >> P = cone excitation coordinates
        // >> XYZ_stimulus = XYZ values of background color
        // >> M = matrix for converting XYZ to cone stimulants (found in Boynton book Ch 6)
        float PL_stimulus = X * 0.15516f + Y * 0.54308f + Z * -0.03287f;
        float PM_stimulus = X * -0.15516f + Y * 0.45692f + Z * 0.03287f;
        float PS_stimulus = X * 0f + Y * 0f + Z * 0.01608f;

        //Debug.Log("Pre PL_stimulus = " + PL_stimulus);
        //Debug.Log("Pre PM_stimulus = " + PM_stimulus);
        //Debug.Log("Pre PS_stimulus = " + PS_stimulus);
        #endregion

        #region Cone Stimulants --> Cone Contrast
        // *NOTE: Cone Contrast as calculated is not used in Brainard's method,
        // but may be used in other methods of DKL conversion

        // CONE EXCITATION COORDINATES FOR BACKGROUND COLOR
        // Pre-calculated in MATLAB using formula similar to the formula used above 
        // for stimulus color conversion: P = XYZ_background * M
        // where,
        // >> P = cone excitation coordinates
        // >> XYZ_background = XYZ values of background color
        // >> M = matrix for converting XYZ to cone stimulants (found in Boynton book Ch 6)
        float PL_background = 120.3848f;
        float PM_background = 63.4342f;
        float PS_background = 3.1915f;

        // CONVERT CONE EXCITATION COORDINATES TO CONE CONTRAST VALUES
        float CL = 0;
        float CM = 0;
        float CS = 0;

        // Input flag indicates to skip cone contrast step
        // Skip conversion for DKL luminance since it is independent of the grey background
        if (flag == 0)
        {
            CL = PL_stimulus;
            CM = PM_stimulus;
            CS = PS_stimulus;
        }

        // Input flag indicates to do cone contrast step
        // Do conversion for DKL hue and saturation values since they are assumed to be on 
        // the equiluminant plane of the grey background
        if (flag == 1)
        {
            CL = (PL_stimulus - PL_background) / PL_background;
            CM = (PM_stimulus - PM_background) / PM_background;
            CS = (PS_stimulus - PS_background) / PS_background;
        }

        //Debug.Log("Pre CL = " + CL);
        //Debug.Log("Pre CM = " + CM);
        //Debug.Log("Pre CS = " + CS);
        #endregion

        #region Cone Stimulants (Cone Excitation Coordinates) --> DKL
        // TRANSFORM CONE EXCITATION COORDINATES TO DKL VALUES
        // calculated using formula: R = P * M
        // where,
        // >> R = DKL values
        // >> P = cone excitation coordinates
        // >> M = matrix for converting cone stimulants to DKL
        // *NOTE: M is pre-calculated separately in MATLAB using code from Brainard chapter

        // For values that underwent cone contrast conversion
        float RL = CL * 0.0094f + CM * 0.0094f + CS * 0f;
        float RLM = CL * 0.0061f + CM * -0.0117f + CS * 0f;
        float RS = CL * -0.0054f + CM * -0.0054f + CS * 0.3133f;

        //Debug.Log("Pre RL = " + RL);
        //Debug.Log("Pre RLM = " + RLM);
        //Debug.Log("Pre RS = " + RS);
        #endregion

        #region DKL --> Spherical Coordinates
        // TRANSFORM DKL VALUES TO SPHERICAL COORDINATES VALUES
        // Use trigonometry
        float vlen = Mathf.Sqrt(Mathf.Pow(RL,2f) + Mathf.Pow(RLM, 2f) + Mathf.Pow(RS, 2f));  // vector length
        float elevation_rads = Mathf.Acos(RL/vlen);
        float azimuth_rads = Mathf.Atan2(RS, RLM);

        // Convert radians to degrees
        float azimuth_deg = (180 / Mathf.PI) * azimuth_rads;        // hue (in deg)
        float elevation_deg = (180 / Mathf.PI) * elevation_rads;    // elevation (in deg)

        //Debug.Log("Pre saturation = " + saturation_len);
        //Debug.Log("Pre azimuth = " + azimuth_deg);
        //Debug.Log("Pre elevation = " + elevation_deg);
        #endregion

        // Put together and return DKL spherical coordinates in an array
        float[] DKLproperties = new float[] { vlen, azimuth_deg, elevation_deg };
        return DKLproperties;
    }


    public int[] DKLtoRGB(float vlen, float azimuth_deg, float elevation_deg, int flag)
    {
        // INPUTS: 1) vector length
        //         2) azimuth angle (in degrees)
        //         3) elevation angle (in degrees)
        //         4) flag variable accepts only 0 or 1: 0 skips cone contrast step (luminance), 
        //              1 does cone contrast step (hue or saturation)

        // OUTPUTS: array of RGB values [1] R value of stimulus color, [2] G value of stimulus 
        //          color, [3] B value of stimulus color

        #region Spherical coords --> DKL
        // TRANSFORM SPHERICAL COORDINATE VALUES TO DKL VALUES
        // Make sure azimuth degree used is valid (no greater than 180 or less than -180)
        if (azimuth_deg > 180)
            azimuth_deg = azimuth_deg - 360;
        if (azimuth_deg < -180)
            azimuth_deg = azimuth_deg + 360;

        // Convert degrees to radians
        float azimuth_rads = (Mathf.PI / 180) * azimuth_deg;        // hue angle (in rad)
        float elevation_rads = (Mathf.PI / 180) * elevation_deg;    // elevation angle (in rad)

        // use inverse trig formulas of ones used in RGBtoDKL conversion
        float RL = vlen * Mathf.Cos(elevation_rads);
        float RLM = vlen * Mathf.Sin(elevation_rads) * Mathf.Cos(azimuth_rads) / 4.5f;    // scale LM axis by dividing by 5
        float RS = vlen * Mathf.Sin(elevation_rads) * Mathf.Sin(azimuth_rads);
        
        outRL = RL;
        outRLM = RLM;
        outRS = RS;

        //Debug.Log("Post RL = " + RL);
        //Debug.Log("Post RLM = " + RLM);
        //Debug.Log("Post RS = " + RS);
        #endregion

        #region DKL --> Cone Stimulants (Cone Excitation Coordinates)
        // TRANSFORM DKL VALUES TO CONE EXCITATION COORDINATES
        // calculated using formula: P = R * M_inv
        // where,
        // >> P = cone excitation coordinates
        // >> R = DKL values
        // >> M_inv = inverse matrix for matrix used to convert cone stimulants to DKL
        // *NOTE: M is pre-calculated separately in MATLAB
        float CL = RL * 69.5042f + RLM * 56.1199f + RS * 0f;
        float CM = RL * 36.6238f + RLM * -56.1199f + RS * 0f;
        float CS = RL * 1.8426f + RLM * 0f + RS * 3.1915f;

        //Debug.Log("Post CL = " + CL);
        //Debug.Log("Post CM = " + CM);
        //Debug.Log("Post CS = " + CS);
        #endregion

        #region Cone Contrast --> Cone Stimulants
        // *NOTE: Cone Contrast as calculated is not used in Brainard's method,
        // but maybe used in other methods of DKL conversion

        // CONE EXCITATION COORDINATES FOR BACKGROUND COLOR
        // Pre-calculated in MATLAB using formula similar to the formula used above 
        // for stimulus color conversion: P = XYZ_background * M
        // where,
        // >> P = cone excitation coordinates
        // >> XYZ_background = XYZ values of background color
        // >> M = matrix for converting XYZ to cone stimulants (found in Boynton book Ch 6)

        float PL_stimulus = 0;
        float PM_stimulus = 0;
        float PS_stimulus = 0;

        // Input flag indicates to skip cone contrast step
        if (flag == 0)
        {
            PL_stimulus = CL;
            PM_stimulus = CM;
            PS_stimulus = CS;
        }

        // Input flag indicates to do cone contrast step
        if (flag == 1)
        {
            float PL_background = 120.3848f;
            float PM_background = 63.4342f;
            float PS_background = 3.1915f;

            // CONVERT CONE CONTRAST VALUES TO CONE EXCITATION COORDINATES
            PL_stimulus = (CL * PL_background) + PL_background;
            PM_stimulus = (CM * PM_background) + PM_background;
            PS_stimulus = (CS * PS_background) + PS_background;
        }

        //Debug.Log("Post PL_stimulus = " + PL_stimulus);
        //Debug.Log("Post PM_stimulus = " + PM_stimulus);
        //Debug.Log("Post PS_stimulus = " + PS_stimulus);
        #endregion

        #region Cone Stimulants to XYZ
        // TRANSFORM CONE EXCITATION COORDINANTES TO XYZ VALUES
        // calculated using formula: XYZ_stimulus = P  * M_inv
        // where,
        // >> XYZ_stimulus = XYZ values of background color
        // >> P = cone excitation coordinates
        // >> M = inverse matrix for matrix used to convert XYZ to cone stimulants (found in Boynton book Ch 6)
        float X = PL_stimulus * 2.94483f + PM_stimulus * -3.50013f + PS_stimulus * 13.17449f;
        float Y = PL_stimulus * 1f + PM_stimulus * 1f + PS_stimulus * 0;
        float Z = PL_stimulus * 0f + PM_stimulus * 0f + PS_stimulus * 62.18905f;

        //Debug.Log("Post X = " + X);
        //Debug.Log("Post Y = " + Y);
        //Debug.Log("Post Z = " + Z);
        #endregion

        #region XYZ to RGB
        // TRANSFORM XYZ VALUES TO RGB VALUES
        // calculated using formula: RGB = XYZ * M
        // where,
        // >> M = inverse matrix for matrix used to convert RGB to XYZ for sRGB (found in sRGB Wiki page)
        float R = X * 3.2408f + Y * -1.5375f + Z * -0.4988f;
        float G = X * -0.9691f + Y * 1.8761f + Z * 0.0417f;
        float B = X * 0.0557f + Y * -0.2041f + Z * 1.0570f;
        #endregion

        int Red = Mathf.Abs(Mathf.RoundToInt(R));
        int Green = Mathf.Abs(Mathf.RoundToInt(G));
        int Blue = Mathf.Abs(Mathf.RoundToInt(B));
        int[] rgb = new int[] { Red, Green, Blue };

        return rgb;
    }
}
