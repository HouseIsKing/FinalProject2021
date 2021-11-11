using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LivingAI : MonoBehaviour
{
    public float Health { get; private set; }
    public bool HasWeapon { get; private set; }
    public GameObject commander;
    public GameObject player;
    public int team;
    public Vector3 targetLocation;
    WeaponSystem weaponSystem;
    Animator animator;
    float deathTimer = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        targetLocation = Vector3.zero;
        HasWeapon = false;
        Health = 100;
        weaponSystem = GetComponent<WeaponSystem>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (deathTimer == 0.0f)
        {
            targetLocation = Vector3.zero;
            Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(100, 10, 100));
            if (HasWeapon)
            {
                if ((commander == null && team != 0))
                {
                    List<LivingAI> livingAIs = new List<LivingAI>();
                    PlayerCharacterController player = null;
                    foreach (Collider collider in colliders)
                    {
                        LivingAI ai = collider.GetComponent<LivingAI>();
                        PlayerCharacterController controller = collider.GetComponent<PlayerCharacterController>();
                        if (ai != null && ai.team != team)
                        {
                            livingAIs.Add(ai);
                        }
                        if (controller != null && team != 0)
                        {
                            player = controller;
                        }
                    }
                    foreach (LivingAI ai in livingAIs)
                    {
                        if (targetLocation != Vector3.zero && Vector3.Distance(transform.position, Vector3.MoveTowards(transform.position, ai.transform.position, 20)) < Vector3.Distance(transform.position, targetLocation))
                        {
                            targetLocation = Vector3.MoveTowards(transform.position, ai.transform.position, 20);
                        }
                        if (targetLocation == Vector3.zero)
                            targetLocation = Vector3.MoveTowards(transform.position, ai.transform.position, 20);
                    }
                    if (player != null)
                    {
                        if (targetLocation != Vector3.zero && Vector3.Distance(transform.position, Vector3.MoveTowards(transform.position, player.transform.position, 20)) < Vector3.Distance(transform.position, targetLocation))
                            targetLocation = Vector3.MoveTowards(transform.position, player.transform.position, 20);
                        if (targetLocation == Vector3.zero)
                            targetLocation = Vector3.MoveTowards(transform.position, player.transform.position, 20);
                    }
                }
                else
                {
                    if (team != 0)
                    {
                        if (commander != null)
                        {
                            targetLocation = commander.transform.position;
                        }
                    }
                    else
                    {
                        targetLocation = player.transform.position;
                    }
                }
                if(weaponSystem.Fire())
                    animator.Play("shot", -1, 20.0f / 30);
            }
            else
            {
                //SearchWeapon
                List<Vector3> weaponVectors = new List<Vector3>();
                foreach (Collider collider in colliders)
                {
                    if (collider.GetComponent<WeaponController>())
                    {
                        weaponVectors.Add(collider.transform.position);
                    }
                }
                foreach (Vector3 vector in weaponVectors)
                {
                    if (targetLocation != Vector3.zero && Vector3.Distance(transform.position, vector) < Vector3.Distance(transform.position, targetLocation))
                    {
                        targetLocation = vector;
                    }
                    if (targetLocation == Vector3.zero)
                        targetLocation = vector;
                }
            }
            if (targetLocation != Vector3.zero)
                GetComponent<NavMeshAgent>().SetDestination(targetLocation);
        }
        else
        {
            GetComponent<NavMeshAgent>().SetDestination(transform.position);
            deathTimer -= Time.deltaTime;
            if (deathTimer < 0)
                Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<WeaponController>())
        {
            weaponSystem.AddWeapon(collision.gameObject.GetComponent<WeaponController>());
            weaponSystem.SwitchToWeaponIndex(0);
            HasWeapon = true;
        }
    }

    public void TakeDmg(float dmg)
    {
        Health -= dmg;
        if(Health <= 0)
        {
            animator.Play("dead");
            deathTimer = 2.0f;
        }
    }
}
