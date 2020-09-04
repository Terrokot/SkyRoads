using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to centrilize control over UI elements.
/// </summary>
public class UIManager : MonoBehaviour
{

    #region PUBLIC VARIABLES
    public delegate void onRestartRequest();
    public event onRestartRequest restartRequest;

    public Text score;
    public Text MessageText;
    public Text highScore;
    public Text timer;
    public Text obstaclesPassed;
    public GameObject uiHolder;
    public Button restartBtn;
    public Text restartBtnLabel;
    #endregion

    #region MEMBER VARIABLES
    private List<Transform> m_UIElements;
    private bool m_TimerStarted = false;
    private float m_StartTime = 0;
    #endregion

    #region CONSTANTS
    private const string START_MESSAGE_TEXT = "Press any key to start playing";
    private const string DEATH_MESSAGE_TEXT = "GAME OVER";
    #endregion

    #region MEMBER METHODS

    //Hide all UI elements.
    private void ResetUI()
    {
        foreach (Transform GO in m_UIElements)
        {
            GO.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (m_TimerStarted)
        {

            UpdateUI();

        }
    }
    #endregion

    #region INIT
    private void Awake()
    {
        m_UIElements = new List<Transform>();
        m_UIElements.AddRange(GetComponentsInChildren<Transform>());
        m_UIElements.Remove(transform);
    }
    #endregion

    #region API
    public void UpdateUI()
    {
        timer.text = (Time.time - m_StartTime).ToString();
        score.text = "SCORE: " + GameController.SessionData.currentScore.ToString();
        highScore.text = "HIGHSCORE: " + GameController.SessionData.highScore.ToString();
    }

    public void ToggleTimer()
    {
        ResetUI();
        m_StartTime = Time.time;
        m_TimerStarted = !m_TimerStarted;
        timer.gameObject.SetActive(m_TimerStarted);
        score.gameObject.SetActive(m_TimerStarted);
        highScore.gameObject.SetActive(m_TimerStarted);
    }

    public void ShowStartMessage()
    {
        ResetUI();
        MessageText.gameObject.SetActive(true);
        MessageText.text = START_MESSAGE_TEXT;
    }

    public void ShowDeathMessage()
    {
        ResetUI();
        MessageText.gameObject.SetActive(true);
        MessageText.text = DEATH_MESSAGE_TEXT;
        restartBtn.gameObject.SetActive(true);
        restartBtnLabel.gameObject.SetActive(true);
    }

    public void RestartBtnCB()
    {
        restartRequest.Invoke();
    }
    #endregion


   
}
