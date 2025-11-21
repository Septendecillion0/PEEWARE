using UnityEngine;

public class Humphrey : Enemy
{
    public float naturalDespawnTime = 5.0f;
    void Start()
    {
        base.NaturallyDespawn(naturalDespawnTime);
    }

    void Update(){
        base.Update();
    }
}
