using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RobotController : MonoBehaviour
{
    // naming constraints do not change
    [SerializeField] private WheelCollider FLWC;
    [SerializeField] private WheelCollider FRWC;
    [SerializeField] private WheelCollider BLWC;
    [SerializeField] private WheelCollider BRWC;

    [SerializeField] private Transform FLWT;
    [SerializeField] private Transform FRWT;
    [SerializeField] private Transform BLWT;
    [SerializeField] private Transform BRWT;

    [SerializeField] private Transform SFR;
    [SerializeField] private Transform SL1;
    [SerializeField] private Transform SL2;
    [SerializeField] private Transform SL3;
    [SerializeField] private Transform SR1;
    [SerializeField] private Transform SR2;
    [SerializeField] private Transform SR3;
    [SerializeField] private Transform SOR;

    [SerializeField] private float maxSteeringAngle = 30;
    [SerializeField] private float motorForce = 50;
    [SerializeField] private float brakeForce;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        float s1x = 0; float s1y = 10; float s1z = 0;
        float s2x = 8; float s2y = 30; float s2z = 0;
        float s3x = 16; float s3y = 60; float s3z = 0;

        positionSensors(SFR, 20, 0, 0);
        positionSensors(SL1, s1x, -s1y, s1z);
        positionSensors(SL2, s2x, -s2y, s2z);
        positionSensors(SL3, s3x, -s3y, s3z);
        positionSensors(SR1, s1x, s1y, s1z);
        positionSensors(SR2, s2x, s2y, s2z);
        positionSensors(SR3, s3x, s3y, s3z);
        positionSensors(SOR, 50, 180, 0);
    }
    
    private Rigidbody rb;
    [SerializeField] private float angle_x;
    [SerializeField] private float angle_z;
    [SerializeField] private float CarVelocity;

    private float steerAngle;
    private bool isBreaking;

    private float s1dist = 5;
    private float s2dist = 6;
    private float s3dist = 6;

    private void positionSensors(Transform sensor, float x_angle, float y_angle, float z_angle)
    {
        sensor.transform.Rotate(x_angle, y_angle, z_angle);
    }

    private void HandleMotor()
    {
        float CurrentAcceleration;

        CurrentAcceleration = isBreaking ? 0 : motorForce;
        FRWC.motorTorque = CurrentAcceleration;
        FLWC.motorTorque = CurrentAcceleration;
        BLWC.motorTorque = CurrentAcceleration;
        BRWC.motorTorque = CurrentAcceleration;

        brakeForce = isBreaking ? 3000f : 0f;
        FLWC.brakeTorque = brakeForce;
        FRWC.brakeTorque = brakeForce;
        BLWC.brakeTorque = brakeForce;
        BRWC.brakeTorque = brakeForce;

    }   
    private void UpdateWheelPosition(WheelCollider Collider, Transform trans)
    {
        Vector3 position;
        Quaternion rotation;
        Collider.GetWorldPose(out position, out rotation);
        trans.position = position;
        trans.rotation = rotation;

    }
    private void UpdateWheels()
    {
        UpdateWheelPosition(FLWC, FLWT);
        UpdateWheelPosition(FRWC, FRWT);
        UpdateWheelPosition(BLWC, BLWT);
        UpdateWheelPosition(BRWC, BRWT);
    }

    private void ControlSteering(float direction)
    {
        steerAngle = direction * maxSteeringAngle;
        FLWC.steerAngle = steerAngle;
        FRWC.steerAngle = steerAngle;
    }

    private bool sense(Transform sensor, float dist, string layer)
               
    {
        LayerMask mask = LayerMask.GetMask(layer);  //the layer mask enables the sensor to detect the road
        if (Physics.Raycast(sensor.position, sensor.TransformDirection(Vector3.forward), dist, mask))
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

    private void MaintainTrack()
    {
        if (!sense(SL3, s3dist, "Road") || !sense(SR3, s3dist, "Road") )
        {
           
            if (!sense(SL3, s3dist, "Road"))
            {
                ControlSteering(1);           
            }
            if (!sense(SR3, s3dist, "Road"))
            {
                ControlSteering(-1);       
            }
        }
        else
        {
            ControlSteering(0);
        }
    }

    private void VarySpeed()

    {
        
        if (CarVelocity < 2 && motorForce <= 50)
        {
            motorForce = motorForce + 340f;        

        }
          
       else if (CarVelocity > 6 && motorForce > 0)
        {
            motorForce = motorForce - 30f;
        }

       else if (CarVelocity > 12 && motorForce < 0)
      {
         motorForce = motorForce -80f;
       }
        
    }
    private void EvadeObstacles()
    {
        if (sense(SL1, s1dist, "Obstacles"))
        {
            ControlSteering(1);
        }
        if (sense(SR1, s1dist, "Obstacles"))
        {
            ControlSteering(-1);
        }
        //
        if (sense(SL2, s2dist, "Obstacles"))
        {
            ControlSteering(1);
        }
        if (sense(SR2, s2dist, "Obstacles"))
        {
            ControlSteering(-1);
        }
       
    }

    private void brake()
    {                                                                                                      
        if ( Input.GetKey(KeyCode.Space)) 
       
           brakeForce =  3000f;
           
       
       else
        brakeForce = 0f;
                                                                                                                     

   }
   private void FixedUpdate()
    {
        MaintainTrack();
        EvadeObstacles();
        VarySpeed();
        HandleMotor();
        UpdateWheels();
        brake();

        angle_x = SOR.eulerAngles.x;
        angle_z = SOR.eulerAngles.z;

        CarVelocity = rb.velocity.magnitude;

    }

}