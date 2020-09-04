using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// helper class used to create the road and obstacles. 
/// Should be improved to use pooling to generate segments of the whole path 
/// to vastly improve preformance. 
/// </summary>
public class LevelGenerator {

    #region PUBLIC VARIABLES
    public GameObject road;

    #endregion

    #region MEMBER VARIABLES
    private Vector3[] m_Path;
    private float m_CurrentSpread = 0;
    private Vector3[] m_Waypoints;
    private Obstacle m_Obstacle;
    private List<Transform> m_RoadGOs;
    private List<Transform> m_ObstaclesGOs;
    private Vector3 m_LastWaypoint = Vector3.zero;
    private int m_CurrentZStep = 0;
    private GameObject m_RoadSegmentHolder;
    #endregion

    #region CONSTANTS
    private const int WAYPOINTS_SEGMENT_LENGHT = 200; 
    private const float DIFFICULTY_TO_SPREAD_MULTIPLIER = 1.5f;
    private const float WAYPOINT_Z_DISTANCE = 20;
    private const float MAX_WAYPOINT_SPREAD = 30;
    private const float CURVE_SMOOTHNESS = 20;
    #endregion

    #region MEMBER METHODS
    //Returns a new waypoint. watpoints are vector3's that go progressively deeper on the Z axis
    //The deeper on the Z axis the waypoints are generated the more spread out they are.
    // An array of waypoints is used to generate to path for the road.
    private Vector3 GetNewWaypoint()
    {
        float newWaypointX = UnityEngine.Random.Range(m_LastWaypoint.x - m_CurrentSpread,
            m_LastWaypoint.x + m_CurrentSpread);
        float newWaypointY = UnityEngine.Random.Range(m_LastWaypoint.x - m_CurrentSpread,
            m_LastWaypoint.x + m_CurrentSpread);
        float newWaypointZ = WAYPOINT_Z_DISTANCE + m_LastWaypoint.z;
        return new Vector3(newWaypointX, newWaypointY, newWaypointZ);
    }

    //Spawns the gameObjects to represent the road. *NOTE: very crude solution.
    //should be using a pooling system here.
    private void InstantiateRoadGO()
    {
        m_RoadGOs = new List<Transform>();
        m_RoadSegmentHolder = new GameObject();
        for (int i = 0; i < m_Path.Length - 1; i++)
        {
            GameObject roadBlockGO = Object.Instantiate(road.gameObject);
            m_RoadGOs.Add(roadBlockGO.transform);
            roadBlockGO.transform.parent = m_RoadSegmentHolder.transform;
            roadBlockGO.transform.position = m_Path[i];
            roadBlockGO.transform.forward = m_Path[i + 1] - m_Path[i];
        }
    }

    private void InstantiateObstacles()
    {
        m_ObstaclesGOs = new List<Transform>();
        for (int i = 50; i < m_Path.Length; i++)
        {
            if (i % (50 - m_CurrentSpread)  == 0)
            {

                GameObject obstacleGO = Object.Instantiate(m_Obstacle.gameObject);
                m_ObstaclesGOs.Add(obstacleGO.transform);
                obstacleGO.transform.parent = m_RoadSegmentHolder.transform;
    
                obstacleGO.transform.position = m_Path[i];
                
                Vector3 obstaclePos = m_Path[i];
                obstaclePos.y += Random.Range(2, 4);
                float obstacleXRandom = road.transform.localScale.x * 0.7f;
                obstaclePos.x += Random.Range(-obstacleXRandom, obstacleXRandom);
                obstacleGO.transform.localPosition = obstaclePos;
                obstacleGO.transform.forward = m_Path[i + 1] - m_Path[i];
            }
        }
    }
    private void UpdateRoadGO()
    {
        for (int i = 0; i < m_RoadGOs.Count; i++)
        {
            m_RoadGOs[i].transform.position = m_Path[i];
            m_RoadGOs[i].transform.forward = m_Path[i + 1] - m_Path[i];
        }
    }

    private void UpdateSpreadAndZStep(int newZStep)
    {
        m_CurrentZStep = newZStep;
        m_CurrentSpread = newZStep * DIFFICULTY_TO_SPREAD_MULTIPLIER;
        if (newZStep * DIFFICULTY_TO_SPREAD_MULTIPLIER > MAX_WAYPOINT_SPREAD)
        {
            m_CurrentSpread = MAX_WAYPOINT_SPREAD;
            return;
        }
    }
    #endregion

    #region API

    //Creates a road with obsticles from the provided prefabs.
    public Vector3[] CreateInitialPath(GameObject roadPrefab, Obstacle obstaclePrefab)
    {
        m_Obstacle = obstaclePrefab;
        road = roadPrefab;
        m_Waypoints = new Vector3[WAYPOINTS_SEGMENT_LENGHT];

        for (int i = 0; i < m_Waypoints.Length; i++)
        {
            Vector3 waypoint = GetNewWaypoint();
            m_Waypoints[i] = waypoint;
            m_LastWaypoint = waypoint;
            UpdateSpreadAndZStep(m_CurrentZStep + 1);
        }
   
        m_Path = Curver.MakeSmoothCurve(m_Waypoints, 20);
        InstantiateRoadGO();
        InstantiateObstacles();
        return m_Path;
    }

    // Base preperation for endless path generation.
    public Vector3[] UpdatePath()
    {
        Debug.Log("ZStep before update is: " + m_CurrentZStep);
        m_Waypoints[0] = m_LastWaypoint;
        for (int i = 1; i < m_Waypoints.Length; i++)
        {
            m_Waypoints[i] = GetNewWaypoint();
            m_LastWaypoint = m_Waypoints[i];

            UpdateSpreadAndZStep(m_CurrentZStep + 1);
        }
        m_Path = Curver.MakeSmoothCurve(m_Waypoints, 20);
        UpdateRoadGO();
        Debug.Log("Road segment updated. Difficulty: " + m_CurrentSpread);
        return m_Path;
    }
    #endregion





   
}
