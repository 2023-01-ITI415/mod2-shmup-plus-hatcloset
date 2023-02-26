using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an enum of the various possible weapon types.
/// It also includes a "shield" type to allow a shield power-up.
/// Items marked [NI] below are Not Implemented in the IGDPD book.
/// </summary>
public enum eWeaponType
{
    none, // The default / no weapons
    blaster, // A simple blaster
    spread, // Two shots simultaneously
    phaser, // [NI] Shots that move in waves
    missile, // [NI] Homing missiles
    laser, // [NI] Damage over time
    shield // Raise shieldLevel
}

/// <summary>
/// The WeaponDefinition class allows you to set the properties
/// of a specific weapon in the Inspector. The Main class has
/// an array of WeaponDefinitions that makes this possible.
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    public string letter; // Letter to show on the power-up
    public Color color = Color.white; // Color of Collar & power-up
    public GameObject weaponModelPrefab; // Prefab for projectiles
    public Color projectileColor = Color.white;
    public float damageOnHit = 0; // Amount of damage caused
    public float continuousDamage = 0; // Damage per second (Laser)
    public float delayBetweenShots = 0;
    public float velocity = 20; // Speed of projectiles
}
public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float nextShotTime; // Time last shot was fired
    private GameObject weaponModel;
    private Transform shotPointTransform;

    //private Renderer collarRend;

    private void Start()
    {
        //collar = transform.Find("Collar").gameObject;
        //collarRend = collar.GetComponent<Renderer>();

        // Dynamically create an anchor for all Projectiles
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTransform = transform.GetChild(0);  //There is only one chile and it is what we need

        // Call SetType() for the default _type of WeaponType.none
        SetType(_type);




        // Find the fireDelegate of the root GameObject
        GameObject rootGO = transform.root.gameObject;

        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;

    }

    public eWeaponType type
    {
        get
        {
            return (_type);
        }
        set
        {
            SetType(value);
        }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;
        if (type == eWeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        //Get the weapon def from main
        def = Main.GET_WEAPON_DEFINITION(_type);

        //Destroy any old model and then attach a model for this weapon type
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0; // You can fire immediately after _type is set.
    }

    public void Fire()
    {
        // If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;
        // If it hasn't been enough time between shots, return
        if (Time.time < nextShotTime) return;


        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            case eWeaponType.spread:
                p = MakeProjectile(); // Make middle Projectile
                p.vel = vel; ;
                p = MakeProjectile(); // Make right Projectile
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile(); // Make left Projectile
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;
        }
    }

    public ProjectileHero MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(def.weaponModelPrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTransform.position;
        pos.z = 0;                                                           
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;                    
        return (p);
    }
}
