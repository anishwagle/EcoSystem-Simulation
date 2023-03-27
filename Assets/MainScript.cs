using Assets;
using FeedForwardWithGeneticAlgorithm;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using UnityEditor;

public class MainScript : MonoBehaviour
{
    public GameObject ai;
    public GameObject food;
    public int radius=20;

    public int aiCount=500;
    public int foodCount=100;

    float nextSpawnTime;
    // Start is called before the first frame update
    void Start()
    {
        var savedStr = PlayerPrefs.GetString("AiList_BAK");
        var ais = JsonConvert.DeserializeObject<List<NeuralNetwork>>(savedStr);
        ais ??= new List<NeuralNetwork>();
        if(ais.Count ==0)
        {
            ais = new List<NeuralNetwork>();
            for (int i = 0; i < aiCount; i++)
            {
                var tran = transform.position;
                tran.z += Random.Range(-1f, 1f) * radius;
                tran.x += Random.Range(-1f, 1f) * radius;
                tran.y = 0f;
                var child = Instantiate(ai, tran, transform.rotation);
                child.name = $"N-0\n{System.Guid.NewGuid().ToString()[..5]}\n";
                var childAiScript = child.GetComponentInChildren<RayCaster>();
                childAiScript.Start();
            }
            

        }
        else
        {
            var counter = 0;
            ais = ais.OrderByDescending(x => x.Fitness).ToList();
            foreach (var ai1 in ais)
            {
                if (counter >= aiCount)
                {
                    break;
                }
                foreach (var ai2 in ais)
                {
                    if(ai1 != ai2)
                    {
                        if (counter >= aiCount)
                        {
                            break;
                        }
                        var tran = transform.position;
                        tran.z += Random.Range(-1f, 1f) * radius;
                        tran.x += Random.Range(-1f, 1f) * radius;
                        tran.y = 0f;
                        var childNN = ai1.CrossOver(ai2);
                        //childNN.Mutation();
                        var child = Instantiate(ai, tran, transform.rotation);
                        var childAiScript = child.GetComponentInChildren<RayCaster>(); 
                        childAiScript.Start();
                        childNN.Generation = ai1.Generation+1; 
                        childAiScript.SetNN(childNN);
                        child.name = $"N-{childNN.Generation}\n{System.Guid.NewGuid().ToString()[..5]}\n";
                        counter++;
                    }
                }
            }
        }

        
        nextSpawnTime = Time.time+ Random.Range(0f,30f) ;
        for(int i = 0; i < foodCount; i++)
        {
            var tran = transform.position;
            tran.z += Random.Range(-1f, 1f) * radius;
            tran.x += Random.Range(-1f, 1f) * radius;
            tran.y = 0.7f;
            Instantiate(food, tran, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var gameObjs = GameObject.FindGameObjectsWithTag("AI");
        if (gameObjs.Length == 0)
        {
            var ais = JsonConvert.DeserializeObject<List<NeuralNetwork>>(PlayerPrefs.GetString("AiList"));
            ais ??= new List<NeuralNetwork>();
            var counter = 0;
            ais = ais.OrderByDescending(x => x.Fitness).ToList();
            foreach (var ai1 in ais)
            {
                if (counter >= aiCount) { break; }

                foreach (var ai2 in ais)
                {
                    if (ai1 != ai2)
                    {
                        if (counter >= aiCount) { break; }
                        var tran = transform.position;
                        tran.z += Random.Range(-1f, 1f) * radius;
                        tran.x += Random.Range(-1f, 1f) * radius;
                        tran.y = 0f;
                        var childNN = ai1.CrossOver(ai2);
                        if (Random.Range(0, 1) < 0.05f)
                        {
                            childNN.Mutation();
                        }
                        
                        var child = Instantiate(ai, tran, transform.rotation);
                        var childAiScript = child.GetComponentInChildren<RayCaster>();
                        childAiScript.Start();
                        childNN.Generation = ai1.Generation+1;
                        childAiScript.SetNN(childNN);
                        child.name = $"N-{childNN.Generation}\n{System.Guid.NewGuid().ToString()[..5]}\n";
                        counter++;
                    }
                }
            }

            PlayerPrefs.SetString("AiList_BAK", JsonConvert.SerializeObject(ais));
            PlayerPrefs.DeleteKey("AiList");
            PlayerPrefs.Save();
        }

        if (Time.time > nextSpawnTime)
        {
            for (int i = 0; i <Random.Range(foodCount/4, foodCount); i++)
            {
                var tran = transform.position;
                tran.z += Random.Range(-1f, 1f) * radius;
                tran.x += Random.Range(-1f, 1f) * radius;
                tran.y = 0.7f;
                Instantiate(food, tran, transform.rotation);
            }


            //increment next_spawn_time
            nextSpawnTime += Random.Range(0f, 30f);
        }
    }
}
