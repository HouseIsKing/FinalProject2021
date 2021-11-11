using UnityEngine;
using UnityEngine.Events;

public class ProjectileBase : MonoBehaviour
{
    public GameObject Owner { get; private set; }
    public Vector3 InitialPosition { get; private set; }
    public Vector3 InitialDirection { get; private set; }
    public Vector3 InheritedMuzzleVelocity { get; private set; }
    public float InitialCharge { get; private set; }

    public UnityAction onShoot;
    float counter = 0;
    public float dmg = 10.0f;
    public bool isGrenade = false;

    private void Update()
    {
        counter += Time.deltaTime;
        if(counter > 10f)
        {
            Destroy(this.gameObject);
        }
    }

    public void Shoot(WeaponController controller)
    {
        Owner = controller.Owner;
        InitialPosition = transform.position;
        InitialDirection = transform.forward;
        InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;
        InitialCharge = controller.CurrentCharge;

        GetComponent<Rigidbody>().AddForce(4000 * InitialDirection);
        if (onShoot != null)
        {
            onShoot.Invoke();
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        LivingAI ai = collision.gameObject.GetComponent<LivingAI>();
        HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>();
        if (ai && !isGrenade)
        {
            ai.TakeDmg(dmg);
        }
        if(healthManager && !isGrenade)
        {
            healthManager.TakeDamage(dmg, gameObject);
        }
    }
}
