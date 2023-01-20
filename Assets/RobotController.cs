// https://www.youtube.com/watch?v=QQs9MWLU_tU

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    // naming constraints do not change
    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    [SerializeField] private Transform SensorFR;
    [SerializeField] private Transform SensorL1;
    [SerializeField] private Transform SensorL2;
    [SerializeField] private Transform SensorL3;
    [SerializeField] private Transform SensorR1;
    [SerializeField] private Transform SensorR2;
    [SerializeField] private Transform SensorR3;
    [SerializeField] private Transform SensorOR;

    [SerializeField] private float maxSteeringAngle;
    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;

    private Rigidbody rb;
        [SerializeField] private float angle_x;
        [SerializeField] private float angle_y;
        [SerializeField] private float angle_z;
        [SerializeField] private float velocity;

        private float steerAngle;
        private bool isBreaking;

        private float s1dist = 5;
        private float s3dist = 4;

    private void AdjustSensors(Transform sensor, float x_angle, float y_angle, float z_angle)
    {
        sensor.transform.Rotate(x_angle, y_angle, z_angle);
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        float s1x = 0; float s1y = 10; float s1z = 0;
         float s3x = 16; float s3y = 50; float s3z = 0;

         AdjustSensors(SensorFR, 20, 0, 0);
         AdjustSensors(SensorL1, s1x, -s1y, s1z);
         AdjustSensors(SensorL3, s3x, -s3y, s3z);
         AdjustSensors(SensorR1, s1x, s1y, s1z);
         AdjustSensors(SensorR3, s3x, s3y, s3z);
         AdjustSensors(SensorOR, 50, 180, 0);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = motorForce;
        frontRightWheelCollider.motorTorque = motorForce;
        rearRightWheelCollider.motorTorque = motorForce;
        rearLeftWheelCollider.motorTorque = motorForce;

        brakeForce = isBreaking ? 3000f : 0f;
        frontRightWheelCollider.brakeTorque = brakeForce;
        frontLeftWheelCollider.brakeTorque = brakeForce;
        rearLeftWheelCollider.brakeTorque = brakeForce;
        rearRightWheelCollider.brakeTorque = brakeForce;

    }

    

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        trans.rotation = rot;
        trans.position = pos;
    }
    private void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void HandleSteering(float direction)

    {
        steerAngle = maxSteeringAngle * direction;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;
    }

    private bool sense(Transform sensor, float dist)
    {
        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.forward), dist))
        {
            Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * dist, Color.yellow);
            return true;
        }
        else
        {
             Debug.DrawRay(sensor.position, sensor.TransformDirection(Vector3.forward) * dist, Color.white);
            return false;
        }
    }
    private void StayOnRoad()
    {
        if (!sense(SensorL3, s3dist)  || !sense(SensorR3, s3dist))
        {
            if (!sense(SensorL3, s3dist))
            {
                HandleSteering(1);
            }
            if (!sense(SensorR3, s3dist))
            {
                HandleSteering(-1);
            }
        }
            else
            {
                HandleSteering(0);
            }
        }
    
    private void AdjustSpeed()
    {
        if (velocity < 2 & motorForce < 50)
        {
            motorForce = motorForce + 10f;
        }
        if (velocity > 4 & motorForce > 0)
        {
            motorForce = motorForce -10f;
        }
    }

    private void AvoidObstacles()
    {
        if (sense(SensorL1, s1dist))
        {
            HandleSteering(1);
        }
        if (sense(SensorR1, s1dist))
        {
            HandleSteering(-1);
        }
    }

    private void FixedUpdate()
    {
        StayOnRoad();
        AvoidObstacles();
        AdjustSpeed();
        HandleMotor();
        UpdateWheels();
        

        angle_x = SensorOR.eulerAngles.x;
        angle_z = SensorOR.eulerAngles.z;

        velocity = rb.velocity.magnitude;
    }
    }



