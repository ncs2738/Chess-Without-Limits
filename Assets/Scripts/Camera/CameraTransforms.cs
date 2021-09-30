using System;
using UnityEngine;

[Serializable]
public class CameraTransforms
{
    [SerializeField]
    Vector3 cameraPosition;
    [SerializeField]
    Vector3 cameraRotation;

    public Vector3 GetPosition()
    {
        return cameraPosition;
    }

    public Vector3 GetRotation()
    {
        return cameraRotation;
    }
}
