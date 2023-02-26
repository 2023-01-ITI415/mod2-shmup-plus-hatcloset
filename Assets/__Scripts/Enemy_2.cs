using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy {

    [Header("Inscribed: Enemy_2")]
    [Tooltip ("Determines how much the sine wave will affect movement")]
    public float sinEccentricity = 0.6f;
    public float lifeTime = 10;
    public AnimationCurve rotCurve;

    [Header("Dynamic: Enemy_2")]
    // Enemy_2 uses a Sin wave to modify a 2-point linear interpolation
    [SerializeField] private Vector3 p0;
    [SerializeField] private Vector3 p1;
    [SerializeField] private float birthTime;
    private Quaternion baseRotation;

    private void Start()
    {
        // Pick any point on the left side of the screen
        p0 = Vector3.zero;
        p0.x = -bndCheck.camWidth - bndCheck.radius;
        p0.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

        // Pick any point on the right side of the screen
        p1 = Vector3.zero;
        p1.x = bndCheck.camWidth + bndCheck.radius;
        p1.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

        // Possibly swap sides
        if (Random.value > 0.5f)
        {
            // Setting the .x of each point to its negative will move it to
            // the other side of the screen
            p0.x *= -1;
            p1.x *= -1;
        }

        // Set the birthTime to the current time
        birthTime = Time.time;

        //Set up initial ship rotation
        transform.position = p0;
        transform.LookAt(p1, Vector3.back);
        baseRotation = transform.rotation;
    }

    public override void Move()
    {
        // Bezier curves work based on a u value between 0 & 1
        float u = (Time.time - birthTime) / lifeTime;

        // If u>1, then it has been longer than lifeTime since birthTime
        if (u > 1)
        {
            // This Enemy_2 has finished its life
            Destroy(this.gameObject);
            return;
        }

        // use the animation curve to set the rotation about Y axis
        float shipRot = rotCurve.Evaluate(u) * 360;
        transform.rotation = baseRotation * Quaternion.Euler(-shipRot, 0, 0);

        // Adjust u by adding a U Curve based on a Sine wave
        u = u + sinEccentricity * (Mathf.Sin(u * Mathf.PI * 2));

        // Interpolate the two linear interpolation points
        pos = ((1 - u) * p0) + (u * p1);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(p0, 1.0f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(p1, Vector3.one);
        Gizmos.color = Color.Lerp(Color.cyan, Color.yellow,
            (Time.time - birthTime) / lifeTime);
        Gizmos.DrawLine(p0, p1);

    }
}
