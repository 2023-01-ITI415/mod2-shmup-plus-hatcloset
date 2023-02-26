using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : MonoBehaviour
{

    [Header("Inscribed: Enemy")]
    public float speed = 10f; // The speed in m/s
    public float fireRate = 0.3f; // Seconds/shot (Unused)
    public float health = 10;
    public int score = 100; // Points earned for destroying this
    public float showDamageDuration = 0.1f; // # seconds to show damage
    public float powerUpDropChance = 1f; // Chance to drop a power-up

    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials;// All the Materials of this & its children
    public bool showingDamage = false;
    public float damageDoneTime; // Time to stop showing damage
    public bool notifiedOfDestruction = false; // Will be used later

    protected BoundsCheck bndCheck;
    protected bool calledShipDestroyed = false;

    // This is a property: A method that acts like a field
    public Vector3 pos
    {
        get
        {
            return (this.transform.position);
        }
        set
        {
            this.transform.position = value;
        }
    }

    private void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        // Get materials and colors for this GameObject and its children
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }



    void Update()
    {
        Move();

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown))
        {
            // We're off the bottom, so destroy this GameObject
            Destroy(gameObject);
        }

    }

    public virtual void Move()
    {
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    private void OnCollisionEnter(Collision coll)
    {
        GameObject otherGO = coll.gameObject;

        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();

        if (p != null) //it's a Projectile Hero
        {

            //If this Enemy is on screen,  damage it.
            if (bndCheck.isOnScreen)
            {
                // Get the damage amount from the Main WEAP_DICT
                health -= Main.GET_WEAPON_DEFINITION(p.type).damageOnHit;
                if (health <= 0)
                {
                    if (!calledShipDestroyed)
                    {
                        calledShipDestroyed = true;
                        Main.SHIP_DESTROYED(this);
                    }
                    Destroy(this.gameObject);
                }
            }
            // Destroy the projectile regardless
            Destroy(otherGO);

        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);
        }
    }
}
