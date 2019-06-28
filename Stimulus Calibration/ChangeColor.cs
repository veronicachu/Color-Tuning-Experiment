using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChangeColor : MonoBehaviour {

    public Material changeMaterial;
    public InputField redInput;
    public InputField greenInput;
    public InputField blueInput;

    private ImportLookup lookupRef;
    private IDictionary<int, byte> redlookup;
    private IDictionary<int, byte> greenlookup;
    private IDictionary<int, byte> bluelookup;

    void Start()
    {
        lookupRef = this.GetComponent<ImportLookup>();

        redInput.text = "255";
        greenInput.text = "255";
        blueInput.text = "255";
    }

    void Update ()
    {
        int redValue = Int32.Parse(redInput.text);
        int greenValue = Int32.Parse(greenInput.text);
        int blueValue = Int32.Parse(blueInput.text);

        // get lookup tables
        redlookup = lookupRef.reddict;
        greenlookup = lookupRef.greendict;
        bluelookup = lookupRef.bluedict;

        //Color32 c = new Color32(redValue, greenValue, blueValue, 255);
        Color32 c = new Color32(redlookup[redValue], greenlookup[greenValue], bluelookup[blueValue], 255);
        changeMaterial.color = c;
    }
}
