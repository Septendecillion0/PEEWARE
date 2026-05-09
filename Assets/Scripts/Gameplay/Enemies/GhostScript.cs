using UnityEngine;
/// <summary>
/// Behavior script for the Mouth Square enemy
/// </summary>
/// <remarks>
/// INTENDED behaviors are as follows:
/// - begin: make sound; slowly move to player
/// 
/// - player reached: trigger jumpscare and despawn
/// 
/// - player gets too far: teleport nearby player, up to a limited number of times
/// 
/// - despawns on scare, after n teleports, or naturally after some time
/// 
/// 
/// future features: 
/// - adjust speed based on player distance or number of teleports
/// - audio/visual fx based on player distance/number of teleports
/// </remarks>
public class GhostScript : Enemy
{
    // overrides written out explicitly for clarity
    public override int id => 4;
    public override bool IsGrounded => false; // Ghost (TODO) has a spawn override but specify groundedness for clarity
    protected override bool naturallyDespawns => true;
    protected override float scareAmount => 20f;
    public override bool uniqueEnemy => false;
    [Header("Behavior")]

    [Header("Behavior Settings")]
    [SerializeField] private float moveSpeed = 0.2f;
    [SerializeField] private float maxAllowedDistance;
    [SerializeField] private int teleportCount = 0;
    [SerializeField] private int maxTeleports = 3;
    [SerializeField] private float teleportRadius;
    [SerializeField] private float teleportRadiusOffset;
    

    protected override void Update()
    {
        base.Update();

        UpdatePosition();
    }

    /// <summary>
    /// Move towards the player, then try to teleport if possible
    /// </summary>
    /// <remarks>
    /// positioning is based on player camera, not actually the player itself
    /// </remarks>
    void UpdatePosition()
    {   
        Transform playerCamTransform = EnemyManager.Instance.playerCam.transform;
        float dist = Vector3.Distance(playerCamTransform.position, transform.position);

        transform.position = Vector3.MoveTowards(
                transform.position, 
                playerCamTransform.position, 
                moveSpeed * Time.deltaTime);

        TryTeleport(dist);
    }

    /// <summary>
    /// Checks teleport conditions and if possible, teleports near the player
    /// If already near the player, doesn't teleport
    /// If not near the player, but no teleports left, despawns
    /// </summary>
    /// <param name="distance"></param>
    void TryTeleport(float distance)
    {
        // Teleport if too far
        if (distance > maxAllowedDistance && teleportCount < maxTeleports)
        {
            TeleportNearPlayer();
        }

        else if (teleportCount >= maxTeleports)
        {
            EnemyManager.Instance.EnemyVanish(this.gameObject);
        }
    }

    void TeleportNearPlayer()
    {
        Transform playerCamTransform = EnemyManager.Instance.playerCam.transform;

        float distance = teleportRadius + Random.Range(-teleportRadiusOffset, teleportRadiusOffset);
        Vector3 direction = Random.onUnitSphere;
        Vector3 pos = playerCamTransform.position + distance * direction;

        transform.position = pos;

        teleportCount += 1;
    }

    void OnTriggerEnter(Collider collision){
        //If the Ghost collided with the player
        // TODO: use something other than tags (layers?)
        if (collision.gameObject.CompareTag("Player")){
            PeeMeterManager.Instance.Scare(scareAmount);
            EnemyDeath();
        }
    }
}
