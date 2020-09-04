using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages ship logic and object
/// </summary>
public class Ship : MonoBehaviour, IPlayerShipInput
{
    #region PUBLIC VARIABLES
    #endregion

    #region EVENTS
    public event GameController.OnShipCollision onShipCollision;
    #endregion

    #region MEMBER VARIABLES
    private Vector3 m_ShipOffset = Vector2.zero;
    private bool m_IsMovingFoward = false;
    private bool m_IsUsingBoost = false;
    private int m_TargetPointOnPathIndex = 1;
    private Transform m_ShipModelTransform;
    private ParticleSystem m_DeathParticles;
    #endregion

    #region CONSTANTS
    private const float SPEED = 22;
    private const float SHIP_OFFSET_BOUNDS = 5.2f; //defines how far the ship can move on X-axis. Should be the X-scale of the road prefab/ 2.
    private const float BOOST_SPEED_MULTIPLIER = 2; 
    private const float X_AXIS_MOVEMENT_SPEED = 8f; 
    #endregion


    #region MEMBER METHODS
    private void Update()
    {
        if (m_IsMovingFoward)
        {
            MoveTowardsNextPoint();
        }
    }

    //Moves the ship Game object along the path.
    //Also applying the previously calculated ship offset.
    private void MoveTowardsNextPoint()
    {
        if (transform.position.z >= GameController.SessionData.path[m_TargetPointOnPathIndex].z
           && m_TargetPointOnPathIndex + 2 < GameController.SessionData.path.Length)
        {
            m_TargetPointOnPathIndex++;
        }

        float finalSpeed = SPEED;

        if (m_IsUsingBoost)
        {
            finalSpeed = SPEED * BOOST_SPEED_MULTIPLIER;
        }

        Vector3 newShipPos = Vector3.MoveTowards(transform.position,
                                                 GameController.SessionData.path[m_TargetPointOnPathIndex] + m_ShipOffset,
                                                 finalSpeed * Time.deltaTime);


        Vector3 newShipFoward = Vector3.RotateTowards(transform.forward,
                                                 (GameController.SessionData.path[m_TargetPointOnPathIndex + 1] + m_ShipOffset) - transform.position,
                                                 finalSpeed * Time.deltaTime,
                                                 0);

        transform.position = newShipPos;
        transform.forward = newShipFoward;
    }
    #endregion

    #region CALLBACKS

    //Collision detection - should only be possible with meteors.
    //Notifies GameController, plays death particles and takes care of disabling the ship
    private void OnTriggerEnter(Collider collision)
    {
        m_IsMovingFoward = false;
        GameObject deathParticlesGO = Instantiate(m_DeathParticles.gameObject);
        deathParticlesGO.transform.position = transform.position;
        deathParticlesGO.GetComponent<ParticleSystem>().Play();
        onShipCollision.Invoke();
        gameObject.SetActive(false);
    }
    #endregion

    #region API

    //Call to setup ship and place ot on initial path.
    public void InitializeShip()
    {
        onShipCollision = new GameController.OnShipCollision(() => { });
        m_ShipModelTransform = GetComponentInChildren<MeshRenderer>().transform;
        m_DeathParticles = GetComponentInChildren<ParticleSystem>();
        transform.position = GameController.SessionData.path[0];
        m_ShipOffset = new Vector3(0, 3, 0);
        transform.position += m_ShipOffset;
    }

    //Call to begin ship flying
    public void StartShipMovement(bool isTrue)
    {
        m_IsMovingFoward = isTrue;

        if (isTrue)
        {
            MoveTowardsNextPoint();
        }
    }

    public void BoostShip(bool isTrue)
    {
        m_IsUsingBoost = isTrue;
    }

    public void MoveLeftRight(int dir)
    {
        float newOffset = m_ShipOffset.x + (X_AXIS_MOVEMENT_SPEED * dir) * Time.deltaTime;
        if (newOffset < SHIP_OFFSET_BOUNDS * -1 && dir == -1)
        {
            newOffset = SHIP_OFFSET_BOUNDS * -1;
        }

        if (newOffset > SHIP_OFFSET_BOUNDS && dir == 1)
        {
            newOffset = SHIP_OFFSET_BOUNDS;
        }

        m_ShipOffset.x = newOffset;

        Vector3 localRot = m_ShipModelTransform.localEulerAngles;
        localRot.z = Mathf.LerpAngle(localRot.z, -dir * 30, 0.1f);
        m_ShipModelTransform.localEulerAngles = localRot;
    }

    //Returns the index of the next point the ship is trying to reach along the path.
    public int GetTargetPointIndex()
    {
        return m_TargetPointOnPathIndex;
    }
    #endregion
}
