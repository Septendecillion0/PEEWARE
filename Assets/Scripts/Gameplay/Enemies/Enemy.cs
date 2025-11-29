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
}
