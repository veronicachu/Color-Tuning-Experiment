using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ExperimentManager_Control : MonoBehaviour
{
    #region Setup Variables
    // Time variables
    public float cueTime;
    public int subtrialNum;
    public float subtrialTime;
    public int trialNumber;
    public float timer;

    // Exp script references
    private ExpSetup_Exp m_ExpSetup;
    private ExpCue_Exp m_ExpCue;
    private ExpTrial_Control m_ExpTrial;
    private LocationData_Control m_LocationData;
    private ResponseData_Exp m_ResponseData;
    private TrialNumber m_TrialNumber;

    // Reference to Photocell flicker object
    private GameObject pcObjectRef;

    // References to UI objects
    private GameObject trialCounterRef;
    private GameObject inputGameobjectRef;
    private GameObject instructionsRef;
    public InputField inputRef;

    // Within trial fucntion call flags
    bool e_cueCall = false;
    List<bool> subTrialCalls = new List<bool>();
    int subtrialInd = 0;
    bool e_respCall = false;
    bool e_flickStartCall = false;

    // Trial progression flag variables
    private bool instructionsDone = false;
    private bool cueDone = false;
    private bool flickerDone = false;
    private bool responseDone = false;

    // Server Manager reference
    [HideInInspector] public int trialStartMarker = 0;
    [HideInInspector] public int trialEndMarker = 0;
    [HideInInspector] public int subtrialMarker = 0;

    // Scene to load
    private int nextSceneNum;

    // Temporary data storage
    private GameObject cueObject;
    private GameObject[] redPeripherals;
    private GameObject[] bluePeripherals;
    private int rand;
    private Color32 targetColor;
    private string targetOrientation;
    private int targetApperances = 0;
    private int resp;
    #endregion

    void Start()
    {
        // Initialize script references
        m_ExpSetup = this.GetComponent<ExpSetup_Exp>();
        m_ExpCue = this.GetComponent<ExpCue_Exp>();
        m_ExpTrial = this.GetComponent<ExpTrial_Control>();
        m_LocationData = this.GetComponent<LocationData_Control>();
        m_ResponseData = this.GetComponent<ResponseData_Exp>();

        // Find peripheral objects
        redPeripherals = GameObject.FindGameObjectsWithTag("RedHUD");
        bluePeripherals = GameObject.FindGameObjectsWithTag("BlueHUD");

        // Find photocell object
        pcObjectRef = GameObject.Find("Photocell");

        // Find instructions object
        instructionsRef = GameObject.Find("Instructions");

        // Setup input field references
        inputGameobjectRef = GameObject.Find("InputField");
        inputGameobjectRef.SetActive(false);

        // Setup trial counter references
        trialCounterRef = GameObject.Find("TrialCounter");
        m_TrialNumber = trialCounterRef.GetComponent<TrialNumber>();
        trialCounterRef.SetActive(false);

        // Setup subTrialCalls bool list
        for (int i = 0; i < subtrialNum; i++)
        {
            subTrialCalls.Add(false);
        }

        nextSceneNum = SceneManager.GetActiveScene().buildIndex + 1;
    }

    void Update()
    {
        // Cheat code to speed through trials for whatever reason
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cueTime = 0.1f;
            subtrialTime = 0.1f;
        }

        // Calls the different stages of the the trial
        if (!instructionsDone && Input.GetKeyDown(KeyCode.Space))
        {
            instructionsRef.SetActive(false);
            instructionsDone = true;
        }
        if (instructionsDone && !cueDone && !flickerDone && !responseDone)
            CuePhase();
        if (instructionsDone && cueDone && !flickerDone && !responseDone)
            TrialPhase();
        if (instructionsDone && cueDone && flickerDone && !responseDone)
            ResponsePhase();
        if (instructionsDone && cueDone && flickerDone && responseDone)
            TrialUpdate();
    }

    void CuePhase()
    {
        // Generates the trial's cue
        if (!e_cueCall)
        {
            // Set peripheral materials
            m_ExpCue.SetPeripheral();

            // Show target object
            cueObject = m_ExpCue.ShowCue();

            // Store the current target's color and orientation temporarily
            targetColor = cueObject.GetComponent<Renderer>().material.color;
            targetOrientation = cueObject.name;

            // Set the flicker frequencies for the trial
            //pcFlickerRef.Frequency = flickControlRef.Frequency; // Set photocell object frequency for the trial

            Debug.Log("cue phase");
            e_cueCall = true;
        }

        // Keeps track of time for when minimum cue phase reached
        timer += Time.deltaTime;
        if (timer >= cueTime && Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            cueDone = true;                         // Sets the cueDone flag to true
            timer = 0f;                             // Reset the timer for the next experiment phase
        }
    }

    void TrialPhase()
    {
        timer += Time.deltaTime;

        // Reset LSL markers
        trialStartMarker = 0;
        trialEndMarker = 0;
        subtrialMarker = 0;

        // Start flickering the HUD
        if (!e_flickStartCall)
        {
            for (int i = 0; i < redPeripherals.Length; i++)
            {
                redPeripherals[i].GetComponent<FlickerControl>().StartFlicker();
                bluePeripherals[i].GetComponent<FlickerControl>().StartFlicker();
            }

            //pcObjectRef.GetComponent<FlickerControl>().StartFlicker();
            m_LocationData.NewFile();

            Debug.Log("trial phase");
            trialStartMarker = 1;
            e_flickStartCall = true;
        }

        // Generates the sub-trials of search arrays
        if (subtrialInd < subtrialNum)
        {
            if (!subTrialCalls[subtrialInd] && timer >= subtrialTime * subtrialInd)
            {
                rand = Random.Range(0, 4);                                  // determines if there will be a target in this subtrial
                if (rand == 0)                                              // add to target count for trial
                    targetApperances++;
                DestroyClones();                                            // destroys previous cubes
                m_ExpTrial.SpawnShapes(rand);                               // generate new search array

                m_LocationData.WriteData();
                subtrialMarker = 1;
                subTrialCalls[subtrialInd] = true;
                subtrialInd = subtrialInd + 1;
            }
        }
        // When full trial time is reached, end the trial
        else if (subtrialInd == subtrialNum && timer >= (subtrialTime * subtrialNum))
        {
            DestroyClones();

            // Turns off the flickering
            for (int i = 0; i < redPeripherals.Length; i++)
            {
                redPeripherals[i].GetComponent<FlickerControl>().StopFlicker();
                bluePeripherals[i].GetComponent<FlickerControl>().StopFlicker();
            }

            m_LocationData.SaveData(targetColor, targetOrientation);

            trialEndMarker = 1;
            timer = 0f;                             // Reset the timer for the next experiment phase
            flickerDone = true;                     // Sets the flickerDone flag to true
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            subtrialInd = subtrialNum;
            timer = subtrialTime * subtrialNum;
        }
    }

    void ResponsePhase()
    {
        if (!e_respCall)
        {
            inputGameobjectRef.SetActive(true);         // show text input field
            inputRef.ActivateInputField();              // activate text input field
            inputRef.text = "";                         // set text input field to be blank

            trialCounterRef.SetActive(true);                                        // show trial counter
            m_TrialNumber.UpdateTrialNumber(trialNumber, m_ExpSetup.totalTrials);   // update trial number

            Debug.Log("Waiting for subject response...");
            e_respCall = true;
        }

        // Wait for the subject's response
        inputRef.Select();                              // select text input field
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (inputRef.text == "")
            {
                Debug.Log("input is empty");
            }
            else if (inputRef.text != "")
            {
                resp = int.Parse(inputRef.text);                            // convert string input to int
                inputRef.DeactivateInputField();                            // deactivate text input field
                inputGameobjectRef.SetActive(false);                        // hide text input field

                // store the trial's info into the trialinfo csv
                m_ResponseData.WriteData(targetColor, targetOrientation, targetApperances, resp);

                responseDone = true;
            }
        }
    }

    void TrialUpdate()
    {
        trialNumber++;                              // Increment the trial number by one
        trialCounterRef.SetActive(false);           // Hide trial counter

        // Closes the experiment out on the last trial
        if (trialNumber == m_ExpSetup.totalTrials)
        {
            m_ResponseData.SaveData();              // Saves the written data to a csv file
            SceneManager.LoadScene(nextSceneNum);              // Switches Trial Scene to End Scene
        }
        else if (Input.GetKey(KeyCode.Escape))
        {
            m_ResponseData.SaveData();              // Saves the written data to a csv file
            SceneManager.LoadScene(nextSceneNum);              // Switches Trial Scene to End Scene
        }

        // Reset all experiment phase progression flags to false for the next trial
        cueDone = false;
        flickerDone = false;
        responseDone = false;

        // Reset target counter
        targetApperances = 0;

        // Reset all calls to exp and flicker code to false for the next trial
        e_cueCall = false;
        for (int i = 0; i < subTrialCalls.Count; i++)
        {
            subTrialCalls[i] = false;
        }
        subtrialInd = 0;
        e_flickStartCall = false;
        e_respCall = false;
    }

    #region Appendix Methods
    private void DestroyClones()
    {
        // Find all the cube clones and destroy them
        GameObject[] arrayClones = GameObject.FindGameObjectsWithTag("Clone");
        for (int i = 0; i < arrayClones.Length; i++)
        {
            Destroy(arrayClones[i]);
        }
    }
    #endregion
}
