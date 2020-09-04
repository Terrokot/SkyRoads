using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment : MonoBehaviour
{

    public float startZ;
    public List<Vector3> rawWaypoints;
    public List<Vector3> waypoints;

    public delegate void OnDespawn(GameObject segment);
    public static event OnDespawn Despawned;

}
