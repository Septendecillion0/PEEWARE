using UnityEngine;

public class GhostScript : Enemy
{
    // overrides written out explicitly for clarity
    public override int id => 4;
    public override bool IsGrounded => false; // Ghost (TODO) has a spawn override but specify groundedness for clarity
    protected override bool naturallyDespawns => true;
    protected override float scareAmount => 20f;
    public override bool uniqueEnemy => true;

    private bool playerLooking = false;

    [Header("Behavior Settings")]
    public float lookThreshold = 0.8f; // dot product threshold
    public float jumpSpeed = 10f;
    public float horizontalOffset = 2f;
    public int tpTime = 3;

    [Header("Distances")]
    public float minBehindDistance = 4f;
    public float maxBehindDistance = 7f;
    public float maxAllowedDistance = 18f;

    protected override void Update()
    {
        base.Update();
        
        Transform playerCam = EnemyManager.Instance.playerCam.transform;
        Vector3 toCreature = (transform.position - playerCam.position).normalized;
        float dot = Vector3.Dot(playerCam.forward, toCreature);

        if (dot > lookThreshold)
        {
            if (Physics.Raycast(playerCam.position, toCreature, out RaycastHit hit, 100f))
            {
                if (hit.transform == transform)
                    playerLooking = true;
            }
        }

        // ✦ Jump scare behavior
        if (playerLooking)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                playerCam.position, 
                jumpSpeed * Time.deltaTime);
        }

        // ✦ Teleport if too far
        float dist = Vector3.Distance(EnemyManager.Instance.player.transform.position, transform.position);
        if (dist > maxAllowedDistance && tpTime > 0)
        {
            TeleportBehindPlayer();
        }
    }

    void TeleportBehindPlayer()
    {
        Transform playerTransform = EnemyManager.Instance.player.transform;
        float behindDist = Random.Range(minBehindDistance, maxBehindDistance);
        Vector3 pos = playerTransform.position - playerTransform.forward * behindDist;
        pos += playerTransform.right * Random.Range(-horizontalOffset, horizontalOffset);
        transform.position = pos;
        tpTime -= 1;
        if (tpTime <= 0)
        {
            EnemyManager.Instance.EnemyVanish(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider collision){
        //If the Ghost collided with the player
        // TODO: use something other than tags (layers)
        if (collision.gameObject.CompareTag("Player")){
            PeeMeterManager.Instance.Scare(scareAmount);
            EnemyDeath();
        }
    }
}
