using UnityEngine;

public class Humphrey : Enemy
{
    public float naturalDespawnTime = 30.0f;
    void Start()
    {
        base.NaturallyDespawn(naturalDespawnTime);
    }

    void Update(){
        base.Update();
    }
}
