using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipCalibration : MonoBehaviour
{
    public int calibrationSceneNum;
    public int controlSceneNum;

	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene(calibrationSceneNum);

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(controlSceneNum);
    }
}
