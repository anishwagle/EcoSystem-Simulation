using FeedForwardWithGeneticAlgorithm;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class RayCaster : MonoBehaviour
{

    public float baseRunningSpeed = 0.5f;
    public float gravity = 20.0f;
    public float lookSpeed = 200.0f;
    public GameObject ai;
    public ParticleSystem muzzelFlash;
    public GameObject partical;

    readonly List<string> TAGS = new() { "Untagged", "AI", "Head", "Food" };
    Vector3 moveDirection = Vector3.zero;
    CharacterController characterController;

    [HideInInspector]
    public NeuralNetwork NN;
    [HideInInspector]
    public int generation;
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public int bonusFitness= 0;

    private bool? changedDirection = false;
    private bool? isForwardDirectionPositive = null;
    private bool? isRightDirectionPositive = null;
    private bool? changedRotation = false;
    private bool? isRotationPositive = null;
    
    private float damage = 5f;
    private float range = 35f;
    
    // Start is called before the first frame update
    public void Start()
    {
        NN ??= new NeuralNetwork(5, 4);
        characterController = ai.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = 0f;
      
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hitInfo, range))
        {
            distance = hitInfo.distance;
        }

        if(hitInfo.transform?.tag == "Food")
        {
            var x = 0;
        }
        var actions = GetActions(ai.transform.position.x, ai.transform.position.y, ai.transform.position.z, distance, hitInfo.transform?.tag);
        
        

        Vector3 forward = ai.transform.TransformDirection(Vector3.forward);
        Vector3 right = ai.transform.TransformDirection(Vector3.right);
        
        float curSpeedX = Mapping(0, 1, -1, 1, (float)actions[0]);
        float curSpeedZ = Mapping(0, 1, -1, 1, (float)actions[1]);
        float rotation = Mapping(0, 1, -1, 1, (float)actions[2]);
        if (Mapping(0, 1, -1, 1, (float)actions[3]) >= 0)
        {
            Shoot();
        }
        speed = Mathf.Abs(curSpeedX) + Mathf.Abs(curSpeedZ) + Mathf.Abs( rotation);
        isForwardDirectionPositive ??= curSpeedX > 0;
        isRightDirectionPositive ??= curSpeedZ > 0;
        isRotationPositive ??= rotation > 0;
        if (isForwardDirectionPositive != (curSpeedX > 0))
        {
            if (changedDirection != null)
            {
                changedDirection = true;
            }
        }
        if (isRightDirectionPositive != (curSpeedZ > 0))
        {
            if (changedDirection != null)
            {
                changedDirection = true;
            }
        }
        if (isRotationPositive != (rotation > 0))
        {
            if (changedRotation != null)
            {
                changedRotation = true;
            }
        }

        if (changedDirection!=null &&changedDirection.Value)
        {
            bonusFitness += 2;
            changedDirection = null;
        }
        if (changedRotation!=null && changedRotation.Value)
        {
            bonusFitness += 2;
            changedRotation = null;
        }
        moveDirection.x = right.x* curSpeedX;
        moveDirection.z = forward.z*curSpeedZ;
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;

        }

        characterController.Move(baseRunningSpeed  * Time.deltaTime * moveDirection);
        moveDirection.y = 0;

        ai.transform.rotation *= Quaternion.Euler(0, rotation * lookSpeed * Time.deltaTime, 0);

    }




    void Shoot()
    {
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hitInfo, range))
        {
            muzzelFlash.Play();
            var impactObject = Instantiate(partical, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(impactObject, 1f);
            if (hitInfo.transform?.tag == "AI")
            {

                var component = hitInfo.transform.GetComponentInParent<Health>();
                component.Damage(damage);
                
            }
        }
    }
    float Mapping(float min, float max, float newMin, float newMax, float value)
    {
        return (value - min) * (newMax - newMin) / (max - min) + newMin;
    }
    List<double> GetActions(float x, float y, float z, float distance, string tag)
    {
        var input = new List<double>() { x, y, z, distance, GetTagNumber(tag) };
        return NN.Calculate(input);
    }
    int GetTagNumber(string tag)
    {
        return TAGS.IndexOf(tag);
    }

    public NeuralNetwork GetChildNN()
    {
        var child = new NeuralNetwork(NN.inputLength, NN.outputLength);
        child.Generation = NN.Generation + 1;
        child.Perceptions = NN.Perceptions.Select(x => x).ToList();
        child.Connections = NN.Connections.Select(x => x).ToList();
        return child;
    }
    public void SetNN(NeuralNetwork nn)
    {
        generation = nn.Generation;
        NN.Generation = generation;
        NN.Perceptions = nn.Perceptions.Select(x => x).ToList();
        NN.Connections = nn.Connections.Select(x => x).ToList();
    }
}
