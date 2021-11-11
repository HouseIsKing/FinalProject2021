using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeExploder : MonoBehaviour
{
    private float timer;
    public GameObject effectObject;
    // Start is called before the first frame update
    void Start()
    {
        timer = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            Instantiate(effectObject, transform.position, transform.rotation);
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10.0f);
            foreach(Collider col in colliders)
            {
                LivingAI ai = col.GetComponent<LivingAI>();
                HealthManager healthManager = col.GetComponent<HealthManager>();
                if (ai)
                {
                    ai.TakeDmg(GetComponent<ProjectileBase>().dmg);
                }
                if(healthManager)
                {
                    healthManager.TakeDamage(GetComponent<ProjectileBase>().dmg, gameObject);
                }
            }
            Destroy(gameObject);
        }
    }
}
