using UnityEngine;
using System.Collections;

public class MakeWaypoints : MonoBehaviour
{
    public int width;
    public int length;
    public float dist;
    public GameObject waypointHolder;
    public Transform fixationLoc;

    private GameObject waypoint;
    private float arrayWidth;
    private float arrayLength;
    private float xCoord;
    private float yCoord;

    void Awake ()
    {
        int newwidth = width + 1;
        int newlength = length + 1;
        arrayWidth = (newwidth * dist) / 2;
        arrayLength = (newlength * dist) / 2;
        
        for (int i = 1; i < newwidth; i++)
        {
            for (int j = 1; j < newlength; j++)
            {
                waypoint = new GameObject("Waypoint" + i + j);
                waypoint.tag = "Waypoints";
                waypoint.transform.parent = waypointHolder.transform;

                xCoord = (dist * i) - arrayWidth;
                yCoord = (dist * j) - arrayLength;

                waypoint.transform.position = new Vector3(xCoord, yCoord, 10f);
                if (waypoint.transform.position == fixationLoc.position)
                    Destroy(waypoint);
            }
        }
	}
}
