using FeedForwardWithGeneticAlgorithm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth;
    private RayCaster aIScript;
    private int foodCount;
    private int requiredFoodCount = 0;
    private int requiredSFoodCount = 5;
    public Slider healthSlider;
    public TextMeshProUGUI name;
    public Canvas canvas;
    public int aiCount = 5;
    private float lifeLivedDays = 0;
    public GameObject ai;
    public GameObject food;
    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        health = maxHealth;
        healthSlider.value = CalculateHealth();
        aIScript = GetComponentInChildren<RayCaster>();
        canvas.worldCamera = camera;
        name.text = $"{gameObject.name}::{lifeLivedDays + aIScript.bonusFitness}::{foodCount}";
    }

    // Update is called once per frame
    void Update()
    {
        
        healthSlider.transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        name.transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);

        
        name.text = $"{gameObject.name}::{Math.Floor( lifeLivedDays) + aIScript.bonusFitness}::{foodCount }";
        lifeLivedDays += Time.deltaTime;
        health -= Time.deltaTime*(1 + aIScript.speed/100);
        
        healthSlider.value = CalculateHealth();
        if (health <= 0)
        {
            var ais = JsonConvert.DeserializeObject<List<NeuralNetwork>>(PlayerPrefs.GetString("AiList"));
            ais ??= new List<NeuralNetwork>();
            
            var ai= aIScript.GetChildNN();
            ai.Fitness = lifeLivedDays +aIScript.bonusFitness;
            ai.Generation = aIScript.generation;
            ais = ais.OrderByDescending(x=>x.Fitness).ToList();
            if(ais.Count < aiCount)
            {
                ais.Add(ai);
                PlayerPrefs.SetString("AiList", JsonConvert.SerializeObject(ais));
                PlayerPrefs.Save();
            }
            else if (ais.Last()?.Fitness < ai.Fitness)
            {
                ais.Remove(ais.Last());
                ais.Add(ai);
                PlayerPrefs.SetString("AiList", JsonConvert.SerializeObject(ais));
                PlayerPrefs.Save();
            }
            
            Destroy(gameObject);
        }
        if(health>maxHealth)
        {
            health = maxHealth;
        }
        
    }
    public void Damage(float damage)
    {
        health -= damage;
        if (health < 0)
        {
            Destroy(gameObject);
            var tran = transform.position;
            tran.y = 0.7f;
            Instantiate(food, tran, transform.rotation);
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
            lifeLivedDays+=Time.deltaTime;
            if (foodCount >= requiredSFoodCount)
            {
                lifeLivedDays += 1;
                foodCount -= requiredSFoodCount;
                var tran = transform.position;
                tran.z = UnityEngine.Random.Range(-1f, 1f) * 40;
                tran.x = UnityEngine.Random.Range(-1f, 1f) * 40;
                tran.y = 0f;
                var parent1AiScript = GetComponentInChildren<RayCaster>();

                var childNN = parent1AiScript.GetChildNN();
                childNN.Mutation();
                var child = Instantiate(ai, tran, transform.rotation);
                var childAiScript = child.GetComponentInChildren<RayCaster>();
                childAiScript.Start();
                //childNN.Generation = parent1AiScript.generation + 1;

                childAiScript.SetNN(childNN);
                child.name = $"S-{childAiScript.generation}\n{System.Guid.NewGuid().ToString()[..5]}\n";
            }
        }

        if (other.gameObject.CompareTag("Head"))
        {
            if(other?.transform?.parent?.GetInstanceID() != transform?.GetInstanceID())
            {
                if(foodCount>=requiredFoodCount)
                {
                    foodCount-=requiredFoodCount;
                    lifeLivedDays += Time.deltaTime+3;

                    health += 35;
                    var tran = transform.position;
                    tran.z += UnityEngine.Random.Range(-1f, 1f) * 80;
                    tran.x += UnityEngine.Random.Range(-1f, 1f) * 80;
                    tran.y = 0f;
                    var parent1AiScript = GetComponentInChildren<RayCaster>();
                    var parent2AiScript = other.transform.parent.GetComponentInChildren<RayCaster>();
                    var nn2 = parent2AiScript.GetChildNN();
                    var nn1 = parent1AiScript.GetChildNN();
                    var childNN = nn1.CrossOver(nn2);
                    childNN.Mutation();
                    var child = Instantiate(ai, tran, transform.rotation);
                    var childAiScript = child.GetComponentInChildren<RayCaster>();
                    childAiScript.Start();
                    childNN.Generation = parent1AiScript.generation + 1;
                    childAiScript.SetNN(childNN);
                    child.name = $"G-{childAiScript.generation}\n{System.Guid.NewGuid().ToString()[..5]}\n";

                }
            }
        }
    }

}
