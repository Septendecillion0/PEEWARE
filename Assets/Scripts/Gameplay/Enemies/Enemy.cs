using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

/// <summary>
/// Base class for all enemies.
/// Handles audio setup, basic player rotation, despawning, and spawn position validation.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Audio")]
    private AudioSource thisAudio;
    public AudioClip passiveSound;
    public AudioClip spawnSound;
    public AudioClip deathSound;

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
    void Awake()
    {
        thisAudio = GetComponent<AudioSource>();
        if (spawnSound != null) thisAudio.PlayOneShot(spawnSound);
        if (passiveSound != null)
        {
            thisAudio.clip = passiveSound;
            thisAudio.loop = true;
            thisAudio.Play();
        }
    }

    // Base Update that always orients toward player
    protected virtual void Update()
    {
        // Always look at the player
        if (EnemyManager.Instance != null && EnemyManager.Instance.player != null)
        {
            Vector3 direction = EnemyManager.Instance.player.transform.position - transform.position;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public void EnemyDeath(){
        if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }

    public void NaturallyDespawn(float age)
    {
        StartCoroutine(Nd(age));
    }

    private IEnumerator Nd(float age)
    {
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
