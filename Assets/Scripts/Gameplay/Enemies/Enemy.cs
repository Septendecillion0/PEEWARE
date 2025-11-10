using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    private AudioSource thisAudio;
    public AudioClip deathSound;

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
        GameObject pl = EnemyManager.Instance.player;
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
