using FeedForwardWithGeneticAlgorithm;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth;
    private AIScript aIScript;
    private int foodCount;
    private int requiredFoodCount = 0;
    private int requiredSFoodCount = 2;
    public Slider healthSlider;
    public TextMeshProUGUI name;

    private int lifeLived = 0;
    private int lifeLivedDays = 0;
    public GameObject ai;
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        healthSlider.value = CalculateHealth();
        aIScript = GetComponent<AIScript>();
        name.text = gameObject.name;
    }

    // Update is called once per frame
    void Update()
    {
        Camera camera = Camera.main;
        healthSlider.transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        name.transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        lifeLivedDays++;
        if(lifeLivedDays == 10000)
        {
            lifeLivedDays = 0;
            lifeLived++;
        }
        
        health -= Time.deltaTime*(1+aIScript.speed);
        healthSlider.value = CalculateHealth();
        if (health <= 0)
        {
            var ais = JsonUtility.FromJson<List<NeuralNetwork>>( PlayerPrefs.GetString("AiList"));
           
            var ai= aIScript.GetNN();
            ai.Fitness = lifeLived;
            ais.OrderBy(x=>x.Fitness).ToList();
            if (ais.Last().Fitness < ai.Fitness)
            {
                ais.Remove(ais.Last());
                ais.Add(ai);
            }
            PlayerPrefs.SetString("AiList",JsonUtility.ToJson(ais));
            PlayerPrefs.Save();
            Destroy(gameObject);
        }
        if(health>maxHealth)
        {
            health = maxHealth;
        }
        
    }
    float CalculateHealth()
    {
        return health / maxHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Food"))
        {
            other.gameObject.SetActive(false);
            health += 10;
            foodCount++;
            if (foodCount >= requiredSFoodCount)
            {

                foodCount -= requiredFoodCount;
                var tran = transform.position;
                tran.z = Random.Range(-1f, 1f) * 40;
                tran.x = Random.Range(-1f, 1f) * 40;
                tran.y = 0f;
                var parent1AiScript = GetComponent<AIScript>();
                parent1AiScript.Start();

                var childNN = parent1AiScript.GetNN();
                childNN.Mutation();
                var child = Instantiate(ai, tran, transform.rotation);
                var childAiScript = child.GetComponent<AIScript>();
                childNN.Generation++;

                childAiScript.SetNN(childNN);
                child.name = $"S-{childNN.Id}";
            }
        }

        if (other.gameObject.CompareTag("Genetal"))
        {
            if(other?.transform?.parent?.GetInstanceID() != transform?.GetInstanceID())
            {
                if(foodCount>=requiredFoodCount)
                {
                    foodCount=foodCount-requiredFoodCount;
                    health += 35;
                    var tran = transform.position;
                    tran.z += Random.Range(-1f, 1f) * 40;
                    tran.x += Random.Range(-1f, 1f) * 40;
                    tran.y = 0f;
                    var parent1AiScript = GetComponent<AIScript>();
                    var parent2AiScript = other.transform.parent.GetComponent<AIScript>();
                    parent1AiScript.Start();
                    parent2AiScript.Start();
                    var nn2 = parent2AiScript.GetNN();
                    var nn1 = parent1AiScript.GetNN();
                    var childNN = nn1.CrossOver(nn2);
                    childNN.Mutation();
                    var child = Instantiate(ai, tran, transform.rotation);
                    var childAiScript = child.GetComponent<AIScript>();
                    childNN.Generation = nn1.Generation+1;
                    childAiScript.SetNN(childNN);
                    child.name = $"G-{childNN.Generation}";

                }
            }
        }
    }

}
