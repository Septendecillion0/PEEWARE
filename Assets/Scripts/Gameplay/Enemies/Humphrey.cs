using UnityEngine;

public class Humphrey : Enemy
{   
    // overrides are default values
    // written out explicitly for clarity
    public override int id => 3;
    public override bool IsGrounded => true;
    protected override bool naturallyDespawns => true;
    public override bool uniqueEnemy => false;

    // All behaviors covered by base class
    
}
