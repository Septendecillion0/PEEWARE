using UnityEngine;
/// <summary>
/// [Enemy]
/// the Fairy is a pair enemy along with the FairyTree
/// the Fairy will try to spawn in front of the player and hover over the camera, blocking the player's vision
/// to "defeat" the Fairy, bring it to its tree
/// colliding the Fairy with an object will kill it, causing it to scream
/// </summary>
public class FairyScript : Enemy
{
    [Header("Extra Audio")]
    public AudioClip fairyLeave;
    private Transform player;
    [Header("Relation to camera")]
    public float disCam = 2f;
    public Vector2 screenOffset = new Vector2(0.5f, 0.5f);
    public float smoothSpeed = 5f;
    
    [Header("Tree")]
    public GameObject treePrefab;
    private GameObject ownTree;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = EnemyManager.Instance.player.transform;
        Vector3 spawnPos = player.position - player.forward * Random.Range(5.0f, 7.0f);
        ownTree = Instantiate(treePrefab, spawnPos, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        //Look at player
        base.Update();
        
        //Slowly move toward middle of the screen
        Vector3 targetPos = playerCam.ViewportToWorldPoint(new Vector3(screenOffset.x, screenOffset.y, disCam));
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision){
        //If collided with the tree, they both vanish ... and suck your pee away
        if (collision.gameObject.tag == "Tree"){
            AudioSource.PlayClipAtPoint(fairyLeave, transform.position, 1f);
            PeeMeterManager.Instance.Scare(-20.0f);
            EnemyManager.Instance.EnemyVanish(this.gameObject);
            EnemyManager.Instance.EnemyVanish(collision.gameObject);
            Debug.Log("fairy reached tree");
        }
        //If not collided with the tree, scare and increase pee
        else{
            PeeMeterManager.Instance.Scare(20.0f);
            EnemyDeath();
            EnemyManager.Instance.EnemyVanish(ownTree);
            Debug.Log("fairy died");
        }
    }
}
