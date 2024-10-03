using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GyroscopeController : MonoBehaviour
{
    public bool Enable;
    [Range(0, 10f)] public float SensitivityTPS = 1.0f;
    [Range(0, 10f)] public float SensitivityFPS = 1.0f;
    public bool InvertControlsTPS = false;
    public bool InvertControlsFPS = false;
    public Camera CameraTPS;
    private Quaternion rot;
    public Transform target;

    // Offset from the target
    public Vector3 offset = new Vector3(0, 5, -10);

    private Quaternion initialRotation;

    private Gyroscope _gyroscope;
    private bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Debug.Log("SUPPORT!");
            _gyroscope = Input.gyro;
            _gyroscope.enabled = true;

            rot = new Quaternion(0, 0, 1, 0);

            return true;
        }

        return false;
    }
    void Start()
    {
        Enable = EnableGyro();
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        if (Enable)
        {
            Quaternion deviceRotation = _gyroscope.attitude * rot;

            Vector3 eulerRotation = deviceRotation.eulerAngles;
            float xRotation = eulerRotation.x * SensitivityTPS;
            float yRotation = eulerRotation.y * SensitivityTPS;

            //CameraTPS.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
            //CameraTPS.transform.rotation = GyroToUnity(_gyroscope.attitude);

            Quaternion gyroAttitude = Input.gyro.attitude;

            Quaternion convertedGyroAttitude = new Quaternion(gyroAttitude.x, gyroAttitude.y, -gyroAttitude.z, -gyroAttitude.w);

            //CameraTPS.transform.localRotation = initialRotation * convertedGyroAttitude;

            Vector3 localDown = Quaternion.Inverse(Input.gyro.attitude) * Vector3.down;

            float rollDegrees = Mathf.Asin(localDown.x) * Mathf.Rad2Deg;

            float pitchDegrees = Mathf.Atan2(localDown.y, localDown.z) * Mathf.Rad2Deg;
            
            Vector3 gyroGravity = Input.gyro.gravity;

            Debug.Log($"tiltUP = {gyroGravity.x} tiltDOWN = {-gyroGravity.x} tiltLeft = {-gyroGravity.y} tiltRight = {gyroGravity.y}");
            //Debug.Log($"rollDegrees = {rollDegrees} , pitchDegrees = {pitchDegrees}");
        }
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
