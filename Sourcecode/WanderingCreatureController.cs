using UnityEngine;
using UnityEngine.AI;

public class WanderingCreatureController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("The maximum distance from the creature's current position within which it can wander.")]
    public float wanderRadius = 10f;
    [Tooltip("The duration the creature waits at its destination before selecting a new destination to wander towards.")]
    public float waitTime = 3f;
    [Tooltip("If enabled, the wait time becomes random within the range [0, Wait Time].")]
    public bool randomTime = false;
    [Tooltip("Toggle whether the creature is allowed to wander or not.")]
    public bool wanderEnabled = true;
    [Tooltip("When enabled, the creature will only wander around its spawn point within a radius defined by the Wander Radius. If disabled, the creature can wander from any point within the Wander Radius.")]
    public bool anchoredWandering = true;

    [Header("Audio")]
    [Tooltip("An array of audio clips that can be played randomly at intervals while the creature is wandering.")]
    public AudioClip[] ambientAudioClips;
    [Tooltip("The minimum interval between playing ambient audio clips.")]
    public float minAmbientAudioInterval = 3f;
    [Tooltip("The maximum interval between playing ambient audio clips.")]
    public float maxAmbientAudioInterval = 7f;
    [Space]
    [Tooltip("An array of audio clips that can be played randomly at intervals while the creature is moving.")]
    public AudioClip[] walkingAudioClips;
    [Tooltip("The interval between playing walking audio clips.")]
    public float walkingAudioInterval = 0.5f;

    [Header("Rotation")]
    [Tooltip("If enabled, the creature will follow the surface normal underneath it.")]
    public bool followSurface = false;
    [Tooltip("The distance to cast the ray to detect the surface normal.")]
    public float surfaceNormalRaycastDistance = 1f;

    private NavMeshAgent agent;
    private bool isMoving = false;
    private float waitTimer = 0f;
    private float ambientAudioTimer = 0f;
    private float walkingAudioTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    void Update()
    {
        if (!wanderEnabled)
            return;

        // Movement logic
        if (isMoving)
        {
            // Check if the agent has reached its destination
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    isMoving = false;
                    if (randomTime)
                    {
                        waitTimer = Random.Range(0f, waitTime);
                    }
                    else
                    {
                        waitTimer = waitTime;
                    }
                }
            }
        }
        else
        {
            // If not moving, decrement wait timer
            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
            }
            else
            {
                // If wait time is over, set new destination
                SetNewDestination();
            }
        }

        // Ambient audio logic
        if (ambientAudioClips != null && ambientAudioClips.Length > 0)
        {
            ambientAudioTimer -= Time.deltaTime;
            if (ambientAudioTimer <= 0f)
            {
                PlayRandomAmbientAudioClip();
                ambientAudioTimer = Random.Range(minAmbientAudioInterval, maxAmbientAudioInterval);
            }
        }

        // Walking audio logic
        if (isMoving && walkingAudioClips != null && walkingAudioClips.Length > 0)
        {
            walkingAudioTimer -= Time.deltaTime;
            if (walkingAudioTimer <= 0f)
            {
                PlayRandomWalkingAudioClip();
                walkingAudioTimer = walkingAudioInterval;
            }
        }

        // Rotate to follow surface normal
        if (followSurface)
        {
            RotateToFollowSurfaceNormal();
        }
    }

    void SetNewDestination()
    {
        // Get a random point within the wander radius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        if (anchoredWandering)
        {
            randomDirection += transform.position;
        }
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
        Vector3 finalPosition = hit.position;

        // Set agent's destination
        agent.SetDestination(finalPosition);
        isMoving = true;
    }

    void PlayRandomAmbientAudioClip()
    {
        int randomIndex = Random.Range(0, ambientAudioClips.Length);
        AudioSource.PlayClipAtPoint(ambientAudioClips[randomIndex], transform.position);
    }

    void PlayRandomWalkingAudioClip()
    {
        int randomIndex = Random.Range(0, walkingAudioClips.Length);
        AudioSource.PlayClipAtPoint(walkingAudioClips[randomIndex], transform.position);
    }

    void RotateToFollowSurfaceNormal()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, surfaceNormalRaycastDistance))
        {
            transform.up = hit.normal;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw the wander radius gizmo
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}