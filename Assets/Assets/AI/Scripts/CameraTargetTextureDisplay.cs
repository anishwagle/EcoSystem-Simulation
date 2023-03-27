using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraTargetTextureDisplay : MonoBehaviour
{

    public Camera targetCamera;
    private RawImage rawImage;
    Camera cam;
    void Start()
    {
        rawImage = GetComponent<RawImage>();
        cam = Camera.main;
        rawImage.enabled = false;
    }

    void Update()
    {
        rawImage.transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);

        if (targetCamera != null)
        {
            rawImage.texture = targetCamera.targetTexture;
        }

        if (Input.GetKey(KeyCode.C))
        {
            rawImage.enabled = !rawImage.enabled;
        }
    }
}
