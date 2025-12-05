using UnityEngine;

public class MouthSquareScript : Enemy
{
    private Transform player;
    private Transform playerCamera;
    private bool playerLooking = false;
    
    public float lookThreshold = 0.95f; // dot product threshold
    public float jumpSpeed = 15f;

    public float naturalDespawnTime = 30.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.NaturallyDespawn(naturalDespawnTime);
    }

    // Update is called once per frame
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
    }

    void OnCollisionEnter(Collision collision){
        //If the Ghost collided with the player
        if (collision.gameObject.tag == "Player"){
            //Jump scare sound needed
            peeMeter.GetComponent<PeeMeterUpdate>().Scare(5.0f);
            EnemyDeath();
        }
    }
}
