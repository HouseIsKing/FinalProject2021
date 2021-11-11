using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class WeaponSystem : MonoBehaviour
{

    [Tooltip("List of weapon the player will start with")]
    public List<WeaponController> startingWeapons = new List<WeaponController>();

    [Header("References")]
    [Tooltip("Parent transform where all weapon will be added in the hierarchy")]
    public Transform weaponParentSocket;
    [Tooltip("Position for weapons when active but not actively aiming")]
    public Transform defaultWeaponPosition;
    [Tooltip("Position for innactive weapons")]
    public Transform downWeaponPosition;

    [Header("Weapon Bob")]
    [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
    public float bobFrequency = 10f;
    [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
    public float bobSharpness = 10f;
    [Tooltip("Distance the weapon bobs when not aiming")]
    public float defaultBobAmount = 0.05f;
    [Tooltip("Distance the weapon bobs when aiming")]
    public float aimingBobAmount = 0.02f;

    [Header("Weapon Recoil")]
    [Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
    public float recoilSharpness = 50f;
    [Tooltip("Maximum distance the recoil can affect the weapon")]
    public float maxRecoilDistance = 0.5f;
    [Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
    public float recoilRestitutionSharpness = 10f;

    [Header("Misc")]
    [Tooltip("Delay before switching weapon a second time, to avoid recieving multiple inputs from mouse wheel")]
    public float weaponSwitchDelay = 1f;
    public int ActiveWeaponIndex { get; private set; }

    public UnityAction<WeaponController> onSwitchedToWeapon;
    public UnityAction<WeaponController, int> onAddedWeapon;
    public WeaponController[] m_WeaponSlots = new WeaponController[1]; // 1 available weapon slots
    Vector3 m_WeaponMainLocalPosition;
    Vector3 m_WeaponRecoilLocalPosition;
    Vector3 m_AccumulatedRecoil;
    int m_WeaponSwitchNewWeaponIndex;

    private void Start()
    {
        ActiveWeaponIndex = -1;

        onSwitchedToWeapon += OnWeaponSwitched;

        // Add starting weapons
        foreach (var weapon in startingWeapons)
        {
            AddWeapon(weapon);
        }
        SwitchWeapon(true);
    }

    public bool Fire()
    {
        WeaponController activeWeapon = GetActiveWeapon();
        if(activeWeapon)
        {
            bool hasFired = activeWeapon.TryShoot();
            if (hasFired)
            {
                m_AccumulatedRecoil += Vector3.back * activeWeapon.recoilForce;
                m_AccumulatedRecoil = Vector3.ClampMagnitude(m_AccumulatedRecoil, maxRecoilDistance);
            }
            return hasFired;
        }
        return false;
    }


    // Update various animated features in LateUpdate because it needs to override the animated arm position
    private void LateUpdate()
    {
        UpdateWeaponRecoil();

        // Set final weapon socket position based on all the combined animation influences
        weaponParentSocket.localPosition = m_WeaponMainLocalPosition + m_WeaponRecoilLocalPosition;
    }

    // Iterate on all weapon slots to find the next valid weapon to switch to
    public void SwitchWeapon(bool ascendingOrder)
    {
        int newWeaponIndex = -1;
        int closestSlotDistance = m_WeaponSlots.Length;
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
            // and select it if it's the closest distance yet
            if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
            {
                int distanceToActiveIndex = GetDistanceBetweenWeaponSlots(ActiveWeaponIndex, i, ascendingOrder);

                if (distanceToActiveIndex < closestSlotDistance)
                {
                    closestSlotDistance = distanceToActiveIndex;
                    newWeaponIndex = i;
                }
            }
        }

        // Handle switching to the new weapon index
        SwitchToWeaponIndex(newWeaponIndex);
    }

    // Switches to the given weapon index in weapon slots if the new index is a valid weapon that is different from our current one
    public void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
    {
        if (force || (newWeaponIndex != ActiveWeaponIndex && newWeaponIndex >= 0))
        {
            // Store data related to weapon switching animation
            m_WeaponSwitchNewWeaponIndex = newWeaponIndex;

            // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
            if (GetActiveWeapon() == null)
            {
                m_WeaponMainLocalPosition = defaultWeaponPosition.localPosition;
                ActiveWeaponIndex = m_WeaponSwitchNewWeaponIndex;

                WeaponController newWeapon = GetWeaponAtSlotIndex(m_WeaponSwitchNewWeaponIndex);
                if (onSwitchedToWeapon != null)
                {
                    onSwitchedToWeapon.Invoke(newWeapon);
                }
            }
        }
    }

    public bool HasWeapon(WeaponController weaponPrefab)
    {
        // Checks if we already have a weapon coming from the specified prefab
        foreach (var w in m_WeaponSlots)
        {
            if (w != null && w.SourcePrefab == weaponPrefab.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    // Updates the weapon recoil animation
    void UpdateWeaponRecoil()
    {
        // if the accumulated recoil is further away from the current position, make the current position move towards the recoil target
        if (m_WeaponRecoilLocalPosition.z >= m_AccumulatedRecoil.z * 0.99f)
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, m_AccumulatedRecoil, recoilSharpness * Time.deltaTime);
        }
        // otherwise, move recoil position to make it recover towards its resting pose
        else
        {
            m_WeaponRecoilLocalPosition = Vector3.Lerp(m_WeaponRecoilLocalPosition, Vector3.zero, recoilRestitutionSharpness * Time.deltaTime);
            m_AccumulatedRecoil = m_WeaponRecoilLocalPosition;
        }
    }

    // Adds a weapon to our inventory
    public bool AddWeapon(WeaponController weaponPrefab)
    {
        // if we already hold this weapon type (a weapon coming from the same source prefab), don't add the weapon
        if (HasWeapon(weaponPrefab))
        {
            return false;
        }

        Destroy(weaponPrefab.gameObject);

        // search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
        for (int i = 0; i < m_WeaponSlots.Length; i++)
        {
            // only add the weapon if the slot is free
            if (m_WeaponSlots[i] == null)
            {
                // spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);
                Destroy(weaponInstance.GetComponent<Rigidbody>());
                Destroy(weaponInstance.GetComponent<MeshCollider>());
                weaponInstance.GetComponent<AudioSource>().enabled = true;
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;

                // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
                weaponInstance.Owner = gameObject;
                weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);
                weaponInstance.transform.localScale *= 20;
                weaponInstance.transform.localRotation = Quaternion.Euler(new Vector3(30, 130, -81));
                m_WeaponSlots[i] = weaponInstance;

                if (onAddedWeapon != null)
                {
                    onAddedWeapon.Invoke(weaponInstance, i);
                }

                return true;
            }
        }

        // Handle auto-switching to weapon if no weapons currently
        if (GetActiveWeapon() == null)
        {
            SwitchWeapon(true);
        }

        return false;
    }

    public WeaponController GetActiveWeapon()
    {
        return GetWeaponAtSlotIndex(ActiveWeaponIndex);
    }

    public WeaponController GetWeaponAtSlotIndex(int index)
    {
        // find the active weapon in our weapon slots based on our active weapon index
        if (index >= 0 &&
            index < m_WeaponSlots.Length)
        {
            return m_WeaponSlots[index];
        }

        // if we didn't find a valid active weapon in our weapon slots, return null
        return null;
    }

    // Calculates the "distance" between two weapon slot indexes
    // For example: if we had 5 weapon slots, the distance between slots #2 and #4 would be 2 in ascending order, and 3 in descending order
    int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
    {
        int distanceBetweenSlots;
        if (ascendingOrder)
        {
            distanceBetweenSlots = toSlotIndex - fromSlotIndex;
        }
        else
        {
            distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);
        }

        if (distanceBetweenSlots < 0)
        {
            distanceBetweenSlots = m_WeaponSlots.Length + distanceBetweenSlots;
        }

        return distanceBetweenSlots;
    }

    void OnWeaponSwitched(WeaponController newWeapon)
    {
        if (newWeapon != null)
        {
            newWeapon.ShowWeapon(true);
        }
    }
}
