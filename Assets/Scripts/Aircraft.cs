using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aircraft : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Aircraft Properties")]
    public float stabilizationFactor;
    public float tailFactor;
    public float liftFactor;
    public float dragFactor;
    public float thrustFactor;
    public float rudderAngle;
    public float elevatorsAngle;
    public float aileronsAngle;
    public AnimationCurve liftCurve;

    [Header("Points")]
    public Transform leftWingForcePoint;
    public Transform rightWingForcePoint;
    public Transform tailForcePoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Debug
        //rb.velocity = Vector3.forward * 300;
    }

    void FixedUpdate()
    {
        // Inputs
        float rudderInput = ((Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0)) * rudderAngle;
        float elevatorsInput = Input.GetAxis("Vertical") * elevatorsAngle;
        float thrustInput = Input.GetKey(KeyCode.Space) ? thrustFactor : 0;
        float aileronsInput = Input.GetAxis("Horizontal") * aileronsAngle;

        Vector3 velocity = rb.velocity;


        Debug.DrawLine(transform.position, transform.position + velocity, Color.white, 0, false);

        Stabilize(velocity);
        TailControl(velocity, rudderInput, elevatorsInput);
        Lift(velocity, aileronsInput);
        Thrust(thrustInput);
        Drag(velocity);
    }

    private void Drag(Vector3 velocity)
    {
        float angleOfAttack = Vector3.Angle(transform.forward, velocity);

        Vector3 dragForce = -velocity.normalized * angleOfAttack * velocity.magnitude * velocity.magnitude * angleOfAttack * dragFactor;

        Debug.DrawLine(transform.position, transform.position + dragForce, Color.blue, 0, false);

        rb.AddForce(dragForce);
    }

    private void Thrust(float thrustInput)
    {
        Vector3 thrustForce = thrustInput * transform.forward;

        Debug.DrawLine(transform.position, transform.position + thrustForce, Color.green, 0, false);

        rb.AddForce(thrustForce);
    }

    private void Lift(Vector3 velocity, float aileronsInput)
    {
        float windSpeed = Vector3.Dot(velocity, transform.forward);
        float angleOfAttack = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(velocity, transform.right), transform.right);

        Vector3 leftLiftForce = transform.up * liftCurve.Evaluate(angleOfAttack + aileronsInput) * windSpeed * windSpeed * liftFactor;
        Vector3 rightLiftForce = transform.up * liftCurve.Evaluate(angleOfAttack - aileronsInput) * windSpeed * windSpeed * liftFactor;

        rb.AddForceAtPosition(leftLiftForce, leftWingForcePoint.position);
        rb.AddForceAtPosition(rightLiftForce, rightWingForcePoint.position);

        Debug.DrawLine(leftWingForcePoint.position, leftWingForcePoint.position + leftLiftForce, Color.red, 0, false);
        Debug.DrawLine(rightWingForcePoint.position, rightWingForcePoint.position + rightLiftForce, Color.red, 0, false);

    }

    // Can merge stabilisation and tail control

    void TailControl(Vector3 velocity, float rudderInput, float elevatorsInput)
    {
        float windSpeed = Vector3.Dot(velocity, transform.forward);

        Vector3 rudderForce = rudderInput * transform.right;
        Vector3 elevatorsForce = elevatorsInput * transform.up;

        Vector3 tailForce = (rudderForce + elevatorsForce) * windSpeed * windSpeed * tailFactor;

        Debug.DrawLine(tailForcePoint.position, tailForcePoint.position + tailForce, Color.yellow, 0, false);

        rb.AddForceAtPosition(tailForce, tailForcePoint.position);
    }

    void Stabilize(Vector3 velocity)
    {
        float yawAngleOfAttack = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(velocity, transform.up), transform.up);
        float pitchAngleOfAttack = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(velocity, transform.right), transform.right);

        Vector3 yawTorque = transform.up * yawAngleOfAttack * velocity.magnitude * velocity.magnitude * stabilizationFactor;
        Vector3 pitchTorque = transform.right * pitchAngleOfAttack * velocity.magnitude * velocity.magnitude * stabilizationFactor;

        rb.AddTorque(yawTorque);
        rb.AddTorque(pitchTorque);
    }
}
