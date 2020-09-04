using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Main game class - initializes all other classes and allows communication between them.
/// </summary>
public class GameController : MonoBehaviour {


    #region PUBLIC VARIABLES
    public GameObject roadPrefab; // NOTE: should be extended to array to support multiple prefab types, same with obstacles.
    public Obstacle obstaclePrefab;

    //Dataholder class with some unused data for future implementation.
    //a readonly version is available for ease of access as a static variable.
    public class SessionProgress
    {
        public float shipZ;
        public float lastPathPointZ;
        public int waypointsGenerated;
        public int highScore;
        public int currentScore;
        public Vector3[] path;

    }

  
    public static SessionProgress SessionData { get => m_SessionData; }
    #endregion

    #region EVENTS & DELEGATES
    //Declaration for event used in ship interface.
    public delegate void OnShipCollision();
    public event OnShipCollision onShipCollision;
    #endregion



    #region MEMBER VARIABLES
    private static SessionProgress m_SessionData;
    private int m_ScoreBuffer = 0;
    private LevelGenerator m_LevelGenerator;
    private SmoothFollow m_SmoothFollow;
    private UIManager m_UIManager;
    private IPlayerShipInput m_ShipInterface;
    private gameState m_State = gameState.startScreen;
    private enum gameState { startScreen, play, death} //Possible gamestates for the gamecotnroller to be in.
    #endregion


    #region INITIALIZTION
    void Start () {
        SwitchGameState(gameState.startScreen);
    }

    private void OnDisable()
    {
        m_ShipInterface.onShipCollision -= OnPlayerDeathCB;
        m_UIManager.restartRequest -= restartGame;
    }

    private void SubscribeToEvents()
    {
        m_ShipInterface.onShipCollision += OnPlayerDeathCB;
        m_UIManager.restartRequest += restartGame;
    }
    #endregion

    #region COROUTINES
    //A coroutine used to update gathered score and save highscores to playerPrefs.
    //*NOTE: Using playerPrefs for longterm user setting storage is not reccommended and an XML file might 
    //be a better solution.
    IEnumerator ScoreCheck()
    {
        while (m_State == gameState.play)
        {
            m_SessionData.currentScore += m_ScoreBuffer;
            if (m_SessionData.currentScore > m_SessionData.highScore)
            {
                m_SessionData.highScore = m_SessionData.currentScore;
                PlayerPrefs.SetInt("Highscore", m_SessionData.highScore);
                PlayerPrefs.Save();
            }
            m_ScoreBuffer = 1;
            yield return new WaitForSeconds(1);
        }
    }
    #endregion

    #region MEMBER METHODS
    private void Update()
    {
        switch (m_State)
        {
            case gameState.startScreen:
                if (Input.anyKey)
                {
                    SwitchGameState(gameState.play);
                }
                break;
            case gameState.play:
                CheckForShipInput();
                break;
            case gameState.death:
                break;
            default:
                break;
        }
    }

    //Execute transition logic for each state.
    private void SwitchGameState(gameState newState)
    {
        switch (newState)
        {
            //initialize game logic.
            case gameState.startScreen:
                if (PlayerPrefs.HasKey("Highscore") == false)
                {
                    PlayerPrefs.SetInt("Highscore", 0);
                    PlayerPrefs.Save();
                }
         
                m_UIManager = FindObjectOfType<UIManager>();
                m_UIManager.ShowStartMessage();
   
                m_ShipInterface = FindObjectOfType<Ship>();
                m_SmoothFollow = Camera.main.GetComponent<SmoothFollow>();
                m_SessionData = new SessionProgress();
                m_SessionData.highScore = PlayerPrefs.GetInt("Highscore");
                m_LevelGenerator = new LevelGenerator();
                m_LevelGenerator.road = roadPrefab;
                SessionData.path = m_LevelGenerator.CreateInitialPath(roadPrefab, obstaclePrefab);
                m_ShipInterface.InitializeShip();
                SubscribeToEvents();
                break;

            case gameState.play:
                m_ShipInterface.StartShipMovement(true);
                m_UIManager.ToggleTimer();
                m_State = newState;
                StartCoroutine(ScoreCheck());
                break;

            case gameState.death:
                m_UIManager.ToggleTimer();
                m_UIManager.ShowDeathMessage();
                break;

            default:
                break;
        }

        m_State = newState;
    }

    private void restartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void CheckForShipInput()
    {

        int inputDirection = 0;
        bool isBoosting = false;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            inputDirection = -1;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            inputDirection = 1;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            isBoosting = true;
        }

        m_ShipInterface.MoveLeftRight(inputDirection);
        m_ShipInterface.BoostShip(isBoosting);

        ApplyCameraZoom(isBoosting);


    }
    
    // Smoothly zoom the camera on boost
    private void ApplyCameraZoom(bool isBoosting)
    {
        if (isBoosting)
        {
            m_SmoothFollow.distance = Mathf.Lerp(m_SmoothFollow.distance, 7, 0.1f);

            m_ScoreBuffer = 5;
        }
        if (!isBoosting)
        {
            m_SmoothFollow.distance = Mathf.Lerp(m_SmoothFollow.distance, 10, 0.1f);
        }
    }

    #endregion

    #region CALLBACKS
    private void OnPlayerDeathCB()
    {
        SwitchGameState(gameState.death);
    }
    #endregion
}
