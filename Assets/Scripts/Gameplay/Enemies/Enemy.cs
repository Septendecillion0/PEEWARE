using UnityEngine;

public class Enemy : MonoBehaviour
{
    private AudioSource thisAudio;
    public AudioClip deathSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnemyDeath(){
        thisAudio.PlayOneShot(deathSound);
        EnemyManager.Instance.EnemyVanish(this.gameObject);
    }
}
