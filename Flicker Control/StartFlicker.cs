using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartFlicker : MonoBehaviour {

    private FlickerControl m_FlickerControl;

	void Start ()
    {
        m_FlickerControl = this.GetComponent<FlickerControl>();
        m_FlickerControl.StartFlicker();
	}
}
