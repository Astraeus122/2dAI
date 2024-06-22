using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public AIAgent controlledAgent;
    public float moveSpeed = 5f;
    private Renderer controlledAgentRenderer;
    private Color originalColor;
    private Rigidbody2D controlledAgentRigidbody;

    void Start()
    {
        controlledAgent = GetComponent<AIAgent>();
        controlledAgentRenderer = GetComponent<Renderer>();
        controlledAgentRigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (controlledAgent != null && controlledAgent.isControlledByPlayer && controlledAgent.currentState != AIAgent.State.Captured && controlledAgent.currentState != AIAgent.State.GoingToPrison)
        {
            HandlePlayerInput();
        }

        if (controlledAgent != null && controlledAgent.isControlledByPlayer)
        {
            CheckFlagCapture();
            CheckEscort();
        }
    }

    void HandlePlayerInput()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow keys
        float moveVertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrow keys

        // Combine horizontal and vertical inputs for movement in the X and Y plane
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        // Move the agent using Rigidbody2D's velocity
        controlledAgentRigidbody.velocity = movement * moveSpeed;
    }

    void CheckFlagCapture()
    {
        if (controlledAgent.carriedFlag != null && IsInOwnTerritory())
        {
            Debug.Log($"{controlledAgent.gameObject.name} has entered its own territory with the flag");
            controlledAgent.CaptureFlag();
        }
    }

    void CheckEscort()
    {
        if (controlledAgent.currentState == AIAgent.State.Escorting && IsInOwnTerritory())
        {
            controlledAgent.escortedAgent.FreeFromPrison();
            controlledAgent.escortedAgent = null;
            controlledAgent.currentState = AIAgent.State.Idle;
            Debug.Log($"{controlledAgent.gameObject.name} has successfully escorted an ally back to their own territory");
        }
    }

    private bool IsInOwnTerritory()
    {
        return (controlledAgent.team == Team.Red && controlledAgent.transform.position.x >= 0) ||
               (controlledAgent.team == Team.Blue && controlledAgent.transform.position.x <= 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        AIAgent enemyAgent = collision.GetComponent<AIAgent>();
        if (enemyAgent != null && enemyAgent.team != controlledAgent.team && enemyAgent.currentState != AIAgent.State.Captured && enemyAgent.currentState != AIAgent.State.GoingToPrison)
        {
            if (IsAgentInMyTerritory(enemyAgent))
            {
                enemyAgent.currentState = AIAgent.State.GoingToPrison;
                enemyAgent.prisonPosition = GetRandomPrisonPosition();
                if (enemyAgent.carriedFlag != null)
                {
                    enemyAgent.carriedFlag.ResetPosition();
                    enemyAgent.carriedFlag.isBeingCarried = false;
                    enemyAgent.carriedFlag = null;
                }
                Debug.Log($"{controlledAgent.gameObject.name} tagged {enemyAgent.gameObject.name} and is sending them to prison");
            }
        }

        Flag flag = collision.GetComponent<Flag>();
        if (flag != null && controlledAgent != null && controlledAgent.currentState != AIAgent.State.Captured && controlledAgent.currentState != AIAgent.State.GoingToPrison)
        {
            controlledAgent.PickUpFlag(flag);
        }

        AIAgent allyAgent = collision.GetComponent<AIAgent>();
        if (allyAgent != null && allyAgent.team == controlledAgent.team && allyAgent.currentState == AIAgent.State.Captured)
        {
            controlledAgent.StartEscorting(allyAgent);
        }
    }

    private bool IsAgentInMyTerritory(AIAgent agent)
    {
        return (controlledAgent.team == Team.Red && agent.transform.position.x > 0) ||
               (controlledAgent.team == Team.Blue && agent.transform.position.x < 0);
    }

    Vector3 GetRandomPrisonPosition()
    {
        BoxCollider2D prisonBounds = controlledAgent.prison.GetComponent<BoxCollider2D>();
        Vector3 minBounds = prisonBounds.bounds.min;
        Vector3 maxBounds = prisonBounds.bounds.max;

        float randomX = Random.Range(minBounds.x, maxBounds.x);
        float randomY = Random.Range(minBounds.y, maxBounds.y);

        return new Vector3(randomX, randomY, transform.position.z);
    }
}
