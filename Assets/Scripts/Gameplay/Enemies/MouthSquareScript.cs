using UnityEngine;
/// <summary>
/// Behavior script for the Mouth Square enemy
/// </summary>
/// <remarks>
/// INTENDED behaviors are as follows:
/// - spawns close to or on walls (TODO)
/// 
/// - passive: produces sound, no movement
/// 
/// - activated when player looks at the mouth square
/// - aggro system: build up when the enemy is ANYWHERE on screen
///                 rate is based on how centered the enemy is (player looking directly at or indirectly at)
///                 decrease when enemy is not on screen
/// - active + aggro: rapidly move towards player camera
///                   stop moving when not being looked at
///                   slowly move back to starting position if aggro decreases enough
/// - upon reaching player, trigger hurt animation + sound, increase pee meter, despawn
/// 
/// potential future expansions: 
/// - change movement speed based on aggro or other params
/// - change behavior to cause guaranteed "hover" state (linger close to player for amount of time)
/// </remarks>
public class MouthSquareScript : Enemy
{
    private Transform player;
    private Transform playerCamera;

    // TODO: fix reference gathering by using EnemyManager; rename to be more accurate since these are transforms and not other GameObjects

    [Header("MouthSquare Attributes")]
    [SerializeField] private float lookThreshold = 0.95f; // dot product threshold
    [SerializeField] private float attackMoveSpeed = 15f;
    [SerializeField] private float returnMoveSpeed = 2f;
    [SerializeField] private float lookIntensity = 0f;
    [SerializeField] private float aggroThreshold = 0.4f; // aggro level at which enemy becomes "active" (movement occurs)
    [SerializeField] private float aggroLevel = 0f;
    [SerializeField] private float aggroBuildSpeed = 2f;
    [SerializeField] private float aggroDecaySpeed = 0.3f;
    [SerializeField] private float naturalDespawnTime = 30.0f;
    [SerializeField] private Vector3 initialPosition;
    
    /// <summary>
    /// Set starting values: initial position and despawn time
    /// </summary>
    void Start()
    {   
        initialPosition = transform.position;
        base.NaturallyDespawn(naturalDespawnTime);

        // note: these are POINTERS and update based on the actual value
        player = EnemyManager.Instance.player.transform;
        playerCamera = EnemyManager.Instance.playerCam.transform;

        Debug.Assert(playerCamera != null, "Player camera not assigned");
    }

    /// <summary>
    /// Every frame, check if it is being viewed, then update aggro and move
    /// </summary>
    void Update()
    {
        base.Update();

        DetectPlayerSight();
        UpdateAggro();
        UpdatePosition();
    }

    /// <summary>
    /// Use raycast to check if player is looking at MouthSquare based on lookThreshold
    /// Updates lookIntensity; 0 = not looking, 1 = looking directly at MouthSquare
    /// </summary>
    private void DetectPlayerSight()
    {
        Vector3 direction = (transform.position - playerCamera.position).normalized;

        float dot = Vector3.Dot(playerCamera.forward, direction);

        // base intensity from how centered on the screen MouthSquare is
        float intensity = Mathf.InverseLerp(lookThreshold, 1f, dot);

        // if not even in front, no need to raycast
        if (intensity <= 0f)
        {
            lookIntensity = 0f;
            return;
        }

        // line-of-sight check
        if (Physics.Raycast(playerCamera.position, direction, out RaycastHit hit, 100f))
        {
            if (hit.transform == transform)
            {
                lookIntensity = intensity;
            }
            else
            {
                lookIntensity = 0f;
            }
        }
        else
        {
            lookIntensity = 0f;
        }
    }

    /// <summary>
    /// Based on the value of lookIntensity, update aggroLevel
    /// </summary>
    private void UpdateAggro()
    {   
        if (lookIntensity > 0f)
        {
            aggroLevel += lookIntensity * aggroBuildSpeed * Time.deltaTime;
        }
        else
        {
            aggroLevel -= aggroDecaySpeed * Time.deltaTime;
        }

        aggroLevel = Mathf.Clamp(aggroLevel, 0f, 1f);
    }

    /// <summary>
    /// Moves MouthSquare based on value of aggroLevel
    /// aggro status is determined based on aggroLevel and aggroThreshold
    /// </summary>
    private void UpdatePosition()
    {   
        // aggro + player looking -> move to player
        if (aggroLevel >= aggroThreshold && lookIntensity > 0f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                playerCamera.position, 
                attackMoveSpeed * Time.deltaTime);
        }
        // aggro + player not looking -> no movement (hover in place)
        else if (aggroLevel >= aggroThreshold)
        {
            return; // no movement
        }
        // not aggro -> move to start position
        else if (aggroLevel < aggroThreshold)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                initialPosition, 
                returnMoveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Check behavior when colliding with something
    /// MouthSquare only checks for player collision, in which case it triggers jumpscare and despawns
    /// ^also must have enough aggro
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision){
        if (collision.gameObject.CompareTag("Player") && aggroLevel >= aggroThreshold){
            peeMeter.GetComponent<PeeMeterUpdate>().Scare(5.0f);
            EnemyManager.Instance.Hurt();
            EnemyDeath();
        }
    }
}
