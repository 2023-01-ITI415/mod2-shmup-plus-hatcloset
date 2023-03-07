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
    public const float DOUBLE_PRESS_TIME = .2f;
    public const float COOLDOWN_TIME = 2f;
    public static Color dashColor = new Vector4(0.4745098f, 0.01960783f, 0.9725491f);
    public static Color dashDoneColor = Color.red;

    [Header("Dynamic")]
    public Color[] originalColors;
    public Material[] materials; // All the Materials of this & its children
    public bool dashedyet = false;
    private float timeSinceLastPress;
    private float lastPressTime;
    private float boost = 0f;
    [Range(0,4)]
    private float _shieldLevel = 1;
    private float _dashLevel = 0;

    

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

        //get Materials and colors for this GameObject and children
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            originalColors[i] = materials[i].color;
        }
    }
	
	// Update is called once per frame
	void Update()
    {
        // Pull in information from the Input class
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        // Change transform.position based on the axes
        Vector3 pos = transform.position;
        pos.x += (xAxis + xAxis * boost) * speed * Time.deltaTime;
        pos.y += (yAxis + yAxis * boost) * speed * Time.deltaTime;
        transform.position = pos;

        // Rotate the ship to make it feel more dynamic
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult * (boost + 1), 0);

        // Use the fireDelegate to fire Weapons
        // First, make sure the button is pressed: Axis("Jump")
        // Then ensure that fireDelegate isn't null to avoid an error
        if (Input.GetAxis("Jump") == 1 && fireEvent != null)
        {
            fireEvent();
            //TempFire();
        }

        float COOLDOWN = COOLDOWN_TIME - (dashLevel * .45f);
        timeSinceLastPress = Time.deltaTime - lastPressTime;

        if (Input.GetButtonDown("Jump"))
        {
            if (dashLevel >= 1 && !dashedyet)
            {
                DashStart();
            } 
        }
        if (boost <= 0f && dashedyet)
        {
            DashLinger();
            Invoke("DashReturn", COOLDOWN);
        }
        if (boost > 0f)
        {
            boost -= 0.02f;
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
            if (boost >= 0.2f) return;
            shieldLevel--;
            dashLevel--;
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
        print("Powerup: " + pUp.type);
        switch (pUp.type)
        {

            case eWeaponType.shield:
                shieldLevel++;
                break;

            case eWeaponType.dash:
                print("dashLevel up!");
                dashLevel++;
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

    public float dashLevel
    {
        get
        {
            return (_dashLevel);
        }
        set
        {
            _dashLevel = Mathf.Min(value, 4);
            // Keep dash from reaching negative values
            if (value < 0)
            {
                value = 0;
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

    void DashStart()
    {
        foreach (Material m in materials)
        {
            m.color = dashColor;
        }
        dashedyet = true;
        boost = 1.5f;
        lastPressTime = Time.deltaTime;
    }
    void DashLinger()
    {
        foreach (Material m in materials)
        {
            m.color = dashDoneColor;
        }
    }
    void DashReturn()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = originalColors[i];
        }
        dashedyet = false;
    }
}
