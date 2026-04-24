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
    public override bool IsGrounded => false; // fairy has a spawn override but specify groundedness for clarity
    [Header("Extra Audio")]
    public AudioClip fairyLeave;

    [Header("Behavior Settings")]
    [SerializeField] private float scareAmountOnCollide = 20f;
    [SerializeField] private float scareAmountOnTree = -20f;
    [SerializeField] private float spawnInvulnerabilityDuration = 2f;

    [Header("Relation to camera")]
    public float disCam = 2f;
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
        
        //Slowly move toward middle of the screen
        Vector3 targetPos = EnemyManager.Instance.playerCam.ViewportToWorldPoint(new Vector3(screenOffset.x, screenOffset.y, disCam));
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collision)
    {
        // Ignore collisions during invulnerability period
        if (invulnerabilityTimer > 0f)
            return;

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
        //If collided with anything else, scare and kill
        else
        {
            PeeMeterManager.Instance.Scare(scareAmountOnCollide);
            EnemyDeath();
            if (ownTree != null)
                EnemyManager.Instance.EnemyVanish(ownTree);
            Debug.Log("fairy died");
        }
    }
}
