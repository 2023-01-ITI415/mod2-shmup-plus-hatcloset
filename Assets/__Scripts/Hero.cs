using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {
    static public Hero S { get; private set; } // Singleton

    [Header("Inscribed")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Dynamic")] [Range(0,4)]
    
    private float _shieldLevel = 1;

    [Tooltip ("This variable holds a reference to the last triggering GameObject")]
    private GameObject lastTriggerGo = null;

    // Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();
    // Create a WeaponFireDelegate field named fireDelegate.
    public WeaponFireDelegate fireEvent;

	void Awake()
    {
        if (S == null)
        {
            S = this; // Set the Singleton
        }
        else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        

        // Reset the weapons to start _Hero with 1 blaster
        ClearWeapons();
        weapons[0].SetType(eWeaponType.blaster);
    }
	
	// Update is called once per frame
	void Update()
    {
        // Pull in information from the Input class
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        // Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        // Rotate the ship to make it feel more dynamic
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

        // Use the fireDelegate to fire Weapons
        // First, make sure the button is pressed: Axis("Jump")
        // Then ensure that fireDelegate isn't null to avoid an error
        if (Input.GetAxis("Jump") == 1 && fireEvent != null)
        {
            fireEvent();
            //TempFire();
        }
    }

   // TempFire - deleted to defer firing to Weapon class

    private void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        //print("Triggered: " + go.name);

        // Make sure it's not the same triggering go as last time
        if (go == lastTriggerGo)
        {
            return;
        }
        lastTriggerGo = go;

        Enemy enemy = go.GetComponent<Enemy>();
        PowerUp pUp = go.GetComponent<PowerUp>();

        if(enemy != null)
        {
            shieldLevel--;
            Destroy(go);
        }
        else if (pUp != null)
        {
            // If the shield was triggered by a PowerUp
            AbsorbPowerUp(pUp);
        }
        else
        {
            print("Triggered by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(PowerUp pUp)
    {
        
        switch (pUp.type)
        {
            case eWeaponType.shield:
                shieldLevel++;
                break;

            default:
                if(pUp.type == weapons[0].type)
                {
                    Weapon w = GetEmptyWeaponSlot();
                    if(w != null)
                    {
                        // Set it to pu.type
                        w.SetType(pUp.type);
                    }
                }
                else
                {
                    //If this is a different weapon type
                    ClearWeapons();
                    weapons[0].SetType(pUp.type);
                }
                break;
        }
        pUp.AbsorbedBy(gameObject);
    }

    public float shieldLevel
    {
        get
        {
            return (_shieldLevel);
        }
        set
        {
            _shieldLevel = Mathf.Min(value, 4);
            // If the shield is going to be set to less than zero
            if (value < 0)
            {
                Destroy(this.gameObject);
                // Tell Main.S to restart the game after a delay
                Main.HERO_DIED();
            }
        }
    }

    Weapon GetEmptyWeaponSlot()
    {
        for (int i=0; i<weapons.Length; i++)
        {
            if (weapons[i].type == eWeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }

    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(eWeaponType.none);
        }
    }
}
