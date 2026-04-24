using UnityEngine;

public class ShadowDoodleScript : Enemy
{
    public override bool IsGrounded => true;
    [Header("Behavior Settings")]
    [SerializeField] private float scareAmountOnTrigger = 10f;

    public float naturalDespawnTime = 30.0f;
    
    void Start()
    {
        base.NaturallyDespawn(naturalDespawnTime);
    }

    void OnTriggerEnter(Collider collision){
        //If player is in its danger zone
        if (collision.gameObject.CompareTag("Player")){
            EnemyManager.Instance.Blinded();
            PeeMeterManager.Instance.Scare(scareAmountOnTrigger);
            EnemyDeath();
        }
    }
}
