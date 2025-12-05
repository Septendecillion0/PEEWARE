using UnityEngine;

public class ShadowDoodleScript : Enemy
{
    public float naturalDespawnTime = 30.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.NaturallyDespawn(naturalDespawnTime);
;    }

    // Update is called once per frame
    void Update()
    {
        //Look at player
        base.Update();
    }

    void OnTriggerEnter(Collider collision){
        //If player is in its danger zone
        if (collision.gameObject.tag == "Player"){
            EnemyManager.Instance.Blinded();
            peeMeter.GetComponent<PeeMeterUpdate>().Scare(20.0f);
            EnemyManager.Instance.EnemyVanish(this.gameObject);
        }
    }
}
