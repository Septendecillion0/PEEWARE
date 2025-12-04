using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private AudioSource thisAudio;
    public AudioClip deathSound;

    [Header("Player Related")]
    private GameObject pl;
    public Camera playerCam;
    public GameObject peeMeter;

    [Header("Spawn Constraints")]

    [Tooltip("Minimum allowed horizontal (XZ plane) distance from player.")]
    public float minHorizontalDistance = 5f;

    [Tooltip("Maximum allowed horizontal (XZ plane) distance from player.")]
    public float maxHorizontalDistance = 20f;

    [Tooltip("Minimum vertical difference allowed between player and spawn position.")]
    public float minVerticalDistance = -2f;

    [Tooltip("Maximum vertical difference allowed between player and spawn position.")]
    public float maxVerticalDistance = 3f;

    [Tooltip("Minimum allowed direction dot-product relative to player forward.")]
    [Range(-1, 1)]
    public float minDirectionDot = -1f;

    [Tooltip("Maximum allowed direction dot-product relative to player forward.")]
    [Range(-1, 1)]
    public float maxDirectionDot = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void Update()
    {
        //Always look at the player
        pl = EnemyManager.Instance.player;
        playerCam = EnemyManager.Instance.playerCam;
        peeMeter = EnemyManager.Instance.peeMeter;
        Vector3 direction = pl.transform.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void EnemyDeath(){
        thisAudio.PlayOneShot(deathSound);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }

    public void NaturallyDespawn(float age){
        StartCoroutine(Nd(age));
    }

    public IEnumerator Nd(float age){
        yield return new WaitForSeconds(age);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }

    public bool IsValidSpawnPosition(Vector3 playerPos, Vector3 playerForward, Vector3 spawnPos)
    {
        Vector3 flatPlayerPos = new Vector3(playerPos.x, 0, playerPos.z);
        Vector3 flatSpawnPos = new Vector3(spawnPos.x, 0, spawnPos.z);

        float horizontalDist = Vector3.Distance(flatPlayerPos, flatSpawnPos);
        if (horizontalDist < minHorizontalDistance || horizontalDist > maxHorizontalDistance)
            return false;

        float verticalDelta = spawnPos.y - playerPos.y;
        if (verticalDelta < minVerticalDistance || verticalDelta > maxVerticalDistance)
            return false;

        Vector3 dirToSpawn = (flatSpawnPos - flatPlayerPos).normalized;
        float dot = Vector3.Dot(playerForward.normalized, dirToSpawn);

        if (dot < minDirectionDot || dot > maxDirectionDot)
            return false;

        return true;
    }
}
