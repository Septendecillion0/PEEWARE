using UnityEngine;

/// <summary>
/// [Enemy]
/// the Fairy is a pair enemy along with the FairyTree
/// the Fairy will try to hover over the camera, blocking the player's vision
/// to "defeat" the Fairy, bring it to its tree
/// colliding the Fairy with an object will kill it, causing it to scream
/// </summary>
public class FairyScript : Enemy
{
    // overrides written out explicitly for clarity
    public override int id => 6;
    public override bool IsGrounded => false;
    protected override bool naturallyDespawns => false;
    protected override float scareAmount => 20f;
    public override bool uniqueEnemy => false; // Tree is the spawn anchor
    
    [Header("Extra Audio")]
    public AudioClip fairyLeave;

    [Header("Behavior Settings")]
    [SerializeField] private float scareAmountOnTree = -20f;
    [SerializeField] private float spawnInvulnerabilityDuration = 2f;

    [Header("Relation to camera")]
    public float hoverDistance = 2f;
    public Vector2 screenOffset = new Vector2(0.5f, 0.5f);
    public float smoothSpeed = 5f;
    
    private GameObject ownTree;
    private float invulnerabilityTimer = 0f;

    /// <summary>
    /// Called by FairyTreeScript to set the tree reference
    /// </summary>
    public void SetTreeReference(GameObject tree)
    {
        ownTree = tree;
        invulnerabilityTimer = spawnInvulnerabilityDuration;
    }

    // Update is called once per frame
    protected override void Update()
    {
        // Always look at the player
        base.Update();
        
        // Update invulnerability timer
        invulnerabilityTimer -= Time.deltaTime;
        
        // Quickly move toward middle of the screen + oscillating offset
        // TODO: replace oscillation with idle animation
        Vector3 oscillation = new Vector3(Mathf.Cos(Time.time), Mathf.Sin(Time.time * 0.9f), 0f) * 0.02f;
        Vector3 targetPos = EnemyManager.Instance.playerCam.ViewportToWorldPoint(new Vector3(screenOffset.x, screenOffset.y, hoverDistance));
        targetPos += oscillation;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        
    }

    /// <summary>
    /// Collision behavior:
    /// - no physics interactions
    /// - If collides with tree: "success", despawn itself and tree, lower pee
    /// - If collides with anything else: "killed", despawn itself and tree, raise pee
    /// </summary>
    /// <remarks>
    /// Requires EnemyTriggerRelay on child
    /// Ignores player collision + has spawn invulnerability to prevent unfair deaths
    /// </remarks>
    /// <param name="collision"></param>
    public override void OnTriggerDetected(Collider collision)
    {
        // Ignore collisions during invulnerability period
        if (invulnerabilityTimer > 0f)
        {
            return;
        }

        //If collided with the tree, they both vanish ... and suck your pee away
        if (collision.gameObject.CompareTag("Tree"))
        {
            AudioSource.PlayClipAtPoint(fairyLeave, transform.position, 1f);
            PeeMeterManager.Instance.Scare(scareAmountOnTree);
            EnemyManager.Instance.EnemyVanish(this.gameObject);
            if (ownTree != null)
                EnemyManager.Instance.EnemyVanish(ownTree);
            Debug.Log("fairy reached tree");
        }
        //If collided with anything else besides player, scare and kill
        else if (!collision.gameObject.CompareTag("Player"))
        {
            PeeMeterManager.Instance.Scare(scareAmount);
            EnemyDeath();
            if (ownTree != null)
                EnemyManager.Instance.EnemyVanish(ownTree);
            Debug.Log("fairy died");
        }
    }
}
