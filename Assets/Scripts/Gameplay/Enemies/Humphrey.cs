using UnityEngine;

public class Humphrey : Enemy
{
    public override bool IsGrounded => true;
    public float naturalDespawnTime = 30.0f;
    
    void Start()
    {
        base.NaturallyDespawn(naturalDespawnTime);
    }
}
