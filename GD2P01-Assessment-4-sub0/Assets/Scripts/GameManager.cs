using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public AIAgent[] redTeamAgents;
    public AIAgent[] blueTeamAgents;
    public Transform redTeamPrison;
    public Transform blueTeamPrison;
    public int flagsToCaptureToWin = 4;

    public GameObject gameOverUI;
    public TextMeshProUGUI redTeamWinText;
    public TextMeshProUGUI blueTeamWinText;
    public Button replayButton;
    public Button menuButton;

    public GameObject pauseMenuUI;

    private int redTeamCapturedFlags = 0;
    private int blueTeamCapturedFlags = 0;

    private AIAgent controlledAgent;
    private Renderer controlledAgentRenderer;
    private Color originalColor;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Time.timeScale = 1;
    }

    void Start()
    {
        gameOverUI.SetActive(false);
        redTeamWinText.gameObject.SetActive(false);
        blueTeamWinText.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        menuButton.gameObject.SetActive(false);

        replayButton.onClick.AddListener(Retry);
        menuButton.onClick.AddListener(ReturnToMainMenu);

        // Ensure all agents' player control is disabled at the start
        foreach (var agent in blueTeamAgents)
        {
            agent.GetComponent<PlayerController>().enabled = false;
        }
    }

    void Update()
    {
        CheckForWin();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        HandleAgentSelection();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void CaptureFlag(Team team)
    {
        if (team == Team.Red)
        {
            redTeamCapturedFlags++;
            Debug.Log($"Red Team captured a flag. Current score: {redTeamCapturedFlags}");
        }
        else if (team == Team.Blue)
        {
            blueTeamCapturedFlags++;
            Debug.Log($"Blue Team captured a flag. Current score: {blueTeamCapturedFlags}");
        }

        CheckForWin();
    }

    void CheckForWin()
    {
        if (redTeamCapturedFlags >= flagsToCaptureToWin)
        {
            redTeamWinText.gameObject.SetActive(true);
            EndGame();
        }
        else if (blueTeamCapturedFlags >= flagsToCaptureToWin)
        {
            blueTeamWinText.gameObject.SetActive(true);
            EndGame();
        }

        int blueTeamInPrison = 0;
        int redTeamInPrison = 0;

        foreach (var agent in blueTeamAgents)
        {
            if (agent.currentState == AIAgent.State.Captured)
            {
                blueTeamInPrison++;
            }
        }

        foreach (var agent in redTeamAgents)
        {
            if (agent.currentState == AIAgent.State.Captured)
            {
                redTeamInPrison++;
            }
        }

        if (blueTeamInPrison >= blueTeamAgents.Length)
        {
            redTeamWinText.gameObject.SetActive(true);
            EndGame();
        }

        if (redTeamInPrison >= redTeamAgents.Length)
        {
            blueTeamWinText.gameObject.SetActive(true);
            EndGame();
        }
    }

    void EndGame()
    {
        gameOverUI.SetActive(true);
        replayButton.gameObject.SetActive(true);
        menuButton.gameObject.SetActive(true);

        foreach (var agent in redTeamAgents)
        {
            agent.enabled = false;
        }

        foreach (var agent in blueTeamAgents)
        {
            agent.enabled = false;
        }

        ReleaseControl();

        Time.timeScale = 0;
    }

    public void Retry()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu"); // Ensure you have a scene named "Main Menu"
    }

    void HandleAgentSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectAgent(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectAgent(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectAgent(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectAgent(3);
        }
    }

    void SelectAgent(int index)
    {
        if (index < 0 || index >= blueTeamAgents.Length)
        {
            return;
        }

        if (controlledAgent != null)
        {
            controlledAgent.isControlledByPlayer = false; // Release previous agent
            controlledAgent.GetComponent<PlayerController>().enabled = false; // Disable player control on the previous agent
            controlledAgent.GetComponent<AIAgent>().enabled = true; // Enable AI on the previous agent
            controlledAgentRenderer.material.color = originalColor; // Reset color
        }

        AIAgent selectedAgent = blueTeamAgents[index];

        // Prevent selection of captured or going to prison agents
        if (selectedAgent.currentState == AIAgent.State.Captured || selectedAgent.currentState == AIAgent.State.GoingToPrison)
        {
            return;
        }

        controlledAgent = selectedAgent;
        controlledAgent.isControlledByPlayer = true; // Control new agent
        controlledAgent.GetComponent<PlayerController>().enabled = true; // Enable player control on the new agent
        controlledAgent.GetComponent<AIAgent>().enabled = false; // Disable AI on the new agent

        // Highlight the selected agent
        controlledAgentRenderer = controlledAgent.GetComponent<Renderer>();
        originalColor = controlledAgentRenderer.material.color;
        controlledAgentRenderer.material.color = Color.yellow; // Highlight color
    }

    public void ReleaseControl()
    {
        if (controlledAgent != null)
        {
            controlledAgent.isControlledByPlayer = false;
            controlledAgent.GetComponent<PlayerController>().enabled = false; // Disable player control
            controlledAgent.GetComponent<AIAgent>().enabled = true; // Enable AI
            controlledAgentRenderer.material.color = originalColor; // Reset color
            controlledAgent = null;
        }
    }
}
