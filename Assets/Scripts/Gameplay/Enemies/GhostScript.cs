using UnityEngine;

public class GhostScript : Enemy
{
    private Transform player;
    private Transform playerCamera;
    private bool playerLooking = false;
    [Header("Distances")]
    public float minBehindDistance = 4f;
    public float maxBehindDistance = 7f;
    public float maxAllowedDistance = 18f;

    [Header("Behavior Settings")]
    public float lookThreshold = 0.8f; // dot product threshold
    public float jumpSpeed = 10f;
    public float horizontalOffset = 2f;
    public int tpTime = 3;

    void Update()
    {
        base.Update();
        player = EnemyManager.Instance.player.transform;
        playerCamera = playerCam.transform;
        Vector3 toCreature = (transform.position - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward, toCreature);

        if (dot > lookThreshold)
        {
            if (Physics.Raycast(playerCamera.position, toCreature, out RaycastHit hit, 100f))
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
                playerCamera.position, 
                jumpSpeed * Time.deltaTime);
        }

        // ✦ Teleport if too far
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > maxAllowedDistance && tpTime > 0)
        {
            //Teleport sound needed
            TeleportBehindPlayer();
        }
    }

    void TeleportBehindPlayer()
    {
        float behindDist = Random.Range(minBehindDistance, maxBehindDistance);
        Vector3 pos = player.position - player.forward * behindDist;
        pos += player.right * Random.Range(-horizontalOffset, horizontalOffset);
        transform.position = pos;
        tpTime -= 1;
        if (tpTime <= 0){
            //Add Vanish sound
            EnemyManager.Instance.EnemyVanish(this.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision){
        //If the Ghost collided with the player
        if (collision.gameObject.tag == "Player"){
            //Jump scare sound needed
            peeMeter.GetComponent<PeeMeterUpdate>().Scare(20.0f);
            EnemyManager.Instance.EnemyVanish(this.gameObject);
        }
    }
}
