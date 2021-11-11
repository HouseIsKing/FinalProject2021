using UnityEngine;

public class Destructable : MonoBehaviour
{
    HealthManager m_Health;

    void Start()
    {
        m_Health = GetComponent<HealthManager>();
        DebugUtility.HandleErrorIfNullGetComponent<HealthManager, Destructable>(m_Health, this, gameObject);

        // Subscribe to damage & death actions
        m_Health.onDie += OnDie;
        m_Health.onDamaged += OnDamaged;
    }

    void OnDamaged(float damage, GameObject damageSource)
    {
        // TODO: damage reaction
    }

    void OnDie()
    {
        // this will call the OnDestroy function
        Destroy(gameObject);
    }
}
