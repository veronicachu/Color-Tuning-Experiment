using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class FlickerControl : MonoBehaviour {

	// Setup variables for controlling flicker color and frequency
	public float Frequency;
    public Material baseMaterial;
    public Material[] materials = new Material[2];
    int textureCounter = 0;
    //RawImage img;
    Renderer objectMaterial;

    // Use this for initialization
    void Start()
    {
        //img = this.GetComponent<RawImage>();
        objectMaterial = this.GetComponent<Renderer>();
    }

    // This method starts the flicker at the given frequency rate
    public void StartFlicker ()
	{
		float freq = (1.0f / (Frequency * 2f));
		InvokeRepeating("CycleColors", freq, freq);
	}

    // This method stops the flicker
    public void StopFlicker()
    {
        CancelInvoke("CycleColors");
        //img.texture = textures[0];
        objectMaterial.material = materials[0];
    }

    // This controls cycling between the two colors
	void CycleColors()
	{
        textureCounter = ++textureCounter % materials.Length;
        //img.texture = textures[textureCounter];
        objectMaterial.material = materials[textureCounter];
	}
}
