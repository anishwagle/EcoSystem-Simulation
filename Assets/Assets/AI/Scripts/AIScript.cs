using FeedForwardWithGeneticAlgorithm;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AIScript : MonoBehaviour
{
    public float baseRunningSpeed = 10f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera camera;
    public float lookSpeed = 200.0f;
    [HideInInspector]
    public NeuralNetwork NN ;
    [HideInInspector]
    public int generation;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    [HideInInspector]
    public bool canMove = true;
    [HideInInspector]
    public float speed;
    // Start is called before the first frame update
    public void Start()
    {
        characterController = GetComponent<CharacterController>();
        var renderTexture = RenderTexture.GetTemporary(new RenderTextureDescriptor(10,10));
        camera.targetTexture= renderTexture;
        NN ??= new NeuralNetwork(camera.targetTexture.width * camera.targetTexture.height, 4);
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
        speed =Mathf.Abs((float) dir[3]);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = Mapping(0, 1, -1, 1, (float)dir[0]);
        float curSpeedZ = Mapping(0, 1, -1, 1, (float)dir[1]);
        moveDirection = (forward * curSpeedX) + (right * curSpeedZ);

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move((baseRunningSpeed * speed) * Time.deltaTime * moveDirection);
        moveDirection.y = 0;


        if (canMove)
        {
            transform.rotation *= Quaternion.Euler(0, Mapping(0, 1, -1, 1, (float)dir[2]) * lookSpeed * Time.deltaTime, 0);
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

    public NeuralNetwork GetChildNN()
    {
        var child = new NeuralNetwork(NN.inputLength,NN.outputLength);
        child.Generation = NN.Generation + 1;
        child.Perceptions = NN.Perceptions.Select(x=>x).ToList();
        child.Connections= NN.Connections.Select(x => x).ToList();
        return child;
    }
    public void SetNN(NeuralNetwork nn)
    {
        generation = nn.Generation;
        NN.Generation = generation;
        NN.Perceptions=nn.Perceptions.Select(x => x).ToList();
        NN.Connections = nn.Connections.Select(x => x).ToList();
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
        Texture2D image = new(camera.targetTexture.width, camera.targetTexture.height);
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

