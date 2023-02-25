using FeedForwardWithGeneticAlgorithm;
using System.Collections.Generic;
using UnityEngine;
using Palmmedia.ReportGenerator.Core.Common;
using JetBrains.Annotations;
using System.Text;
using System.Threading.Tasks;

[RequireComponent(typeof(CharacterController))]
public class AIScript : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 10f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera camera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    NeuralNetwork NN ;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    [HideInInspector]
    public bool canMove = true;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        NN = new NeuralNetwork(camera.targetTexture.width * camera.targetTexture.height, 3);
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;



    }
    float Mapping(float min, float max, float newMin, float newMax, float value)
    {
        return (value - min) * (newMax - newMin) / (max - min) + newMin;
    }
    // Update is called once per frame
    void Update()
    {
        var dir = GetDirections();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = Mapping(0, 1, -1, 1, (float)dir[0]);
        float curSpeedZ = Mapping(0, 1, -1, 1, (float)dir[1]);
        moveDirection = (forward * curSpeedX) + (right * curSpeedZ);

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        characterController.Move(moveDirection * runningSpeed * Time.deltaTime);
        moveDirection.y = 0;


        if (canMove)
        {
            rotationX += -Mapping(0, 1, -1, 1, (float)dir[2]) * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            // playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Mapping(0, 1, -1, 1, Mathf.PerlinNoise(Time.time, 0.0f)) * lookSpeed, 0);
        }
    }

     List<double> GetDirections()
    {
        var img = RTImage();
        var input = new List<double>();
        for (var i = 0; i < camera.targetTexture.width; i++)
        {
            for (var j = 0; j < camera.targetTexture.height; j++)
            {
                var pix = img.GetPixel(i, j);

                input.Add(GetGrayScale(pix.r,pix.g,pix.b));
            }
        }
        var dir = NN.Calculate(input);
        return dir;
    }

    Texture2D RTImage()
    {
        // The Render Texture in RenderTexture.active is the one
        // that will be read by ReadPixels.
        var currentRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        // Render the camera's view.
        camera.Render();

        // Make a new texture and read the active Render Texture into it.
        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        // Replace the original active Render Texture.
        RenderTexture.active = currentRT;
        return image;
    }
    double GetGrayScale(float r,float g,float b)
    {
        return (r+g+b)/3;
    }
   
}

