using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Tooltip("Multiplier to apply to the received damage")]
    public float damageMultiplier = 1f;
    [Range(0, 1)]
    [Tooltip("Multiplier to apply to self damage")]
    public float sensibilityToSelfdamage = 0.5f;

    public HealthManager Health { get; private set; }

    void Awake()
    {
        // find the health component either at the same level, or higher in the hierarchy
        Health = GetComponent<HealthManager>();
        if (!Health)
        {
            Health = GetComponentInParent<HealthManager>();
        }
    }

    public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
    {

        if(Health)
        {
            var totalDamage = damage;

            // skip the crit multiplier if it's from an explosion
            if (!isExplosionDamage)
            {
                totalDamage *= damageMultiplier;
            }

            // potentially reduce damages if inflicted by self
            if (Health.gameObject == damageSource)
            {
                totalDamage *= sensibilityToSelfdamage;
            }

            // apply the damages
            Health.TakeDamage(totalDamage, damageSource);
        }
    }
}
