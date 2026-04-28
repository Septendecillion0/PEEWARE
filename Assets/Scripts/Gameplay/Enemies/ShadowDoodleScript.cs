using UnityEngine;
/// <summary>
/// Main Shadow Doodle behavior script
/// - Ground-based
/// - Physics active
/// - Stationary
/// Mechanics:
/// - If player enters trigger zone, start raising darkness
/// - If player exits trigger zone, start lowering darkness
/// - If darkness reaches 1, cause blind, raise pee, despawn
/// </summary>
/// <remarks>
/// Each ShadowDoodle holds its own darkness value
/// TODO: make darkness playerattribute or shared across shadows
/// TODO: replace blind animation and darkness visuals
/// </remarks>
public class ShadowDoodleScript : Enemy
{
    // overrides are default values
    // written out explicitly for clarity
    public override int id => 2;
    public override bool IsGrounded => true;
    protected override bool naturallyDespawns => true;
    protected override float scareAmount => 10f;
    public override bool uniqueEnemy => false;

    [Header("ShadowDoodle Behavior")]
    [SerializeField] private bool playerInside = false;
    [SerializeField] private float darkness = 0f; // Scales from 0 to 1
    [SerializeField] private float darknessRate = 0.4f;

    /// <summary>
    /// Update darkness value based on player presence
    /// - also draws dark overlay
    /// Trigger blind if darkness is high enough
    /// 
    /// TODO: darkness as global value, rework and change how UI handles it
    /// </summary>
    protected override void Update()
    {
        base.Update();

        float darknessDelta = playerInside ? darknessRate * Time.deltaTime : - darknessRate * Time.deltaTime;
        UIManager.Instance.UpdateDarkness(darknessDelta);

        darkness = Mathf.Clamp(darkness + darknessDelta, 0f, 1f);

        if (Mathf.Approximately(darkness, 1f))
        {
            PeeMeterManager.Instance.Scare(scareAmount);
            EnemyManager.Instance.Blinded();
            EnemyDeath();
        }
    }

    /// <summary>
    /// Detect player enter trigger zone, set bool
    /// </summary>
    /// <param name="collision"></param>
    public override void OnTriggerDetected(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    /// <summary>
    /// Detect player exit trigger zone, reset bool
    /// </summary>
    /// <param name="collision"></param>
    public override void OnTriggerExitDetected(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}
