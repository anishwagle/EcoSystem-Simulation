using FeedForwardWithGeneticAlgorithm;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public GameObject ai;
    public GameObject food;
    public int radius=20;

    public int aiCount=30;
    public int foodCount=100;

    float nextSpawnTime;
    // Start is called before the first frame update
    void Start()
    {
        var ais = JsonUtility.FromJson<List<NeuralNetwork>>(PlayerPrefs.GetString("AiList"));
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
                child.name = "firstGen";
                var childAiScript = child.GetComponent<AIScript>();
                childAiScript.Start();
                ais.Add(childAiScript.GetNN());
            }
            PlayerPrefs.SetString("AiList", JsonUtility.ToJson(ais));
            PlayerPrefs.Save();
        }
        else
        {
            var counter = 0;
            var tran = transform.position;
            tran.z += Random.Range(-1f, 1f) * 40;
            tran.x += Random.Range(-1f, 1f) * 40;
            tran.y = 0f;
            foreach (var ai1 in ais)
            {
                foreach(var ai2 in ais)
                {
                    if(ai1 != ai2)
                    {
                        if(counter >= aiCount) { break; }

                        var childNN = ai1.CrossOver(ai2);
                        childNN.Mutation();
                        var child = Instantiate(ai, tran, transform.rotation);
                        var childAiScript = child.GetComponent<AIScript>();
                        childNN.Generation = ai1.Generation+1;
                        childAiScript.SetNN(childNN);
                        child.name = $"G-{childNN.Generation}";
                        counter++;
                    }
                }
            }
        }

        
        nextSpawnTime = Time.time+ Random.Range(0f,10f) ;
        for(int i = 0; i < foodCount; i++)
        {
            var tran = transform.position;
            tran.z += Random.Range(-1f, 1f) * radius;
            tran.x += Random.Range(-1f, 1f) * radius;
            tran.y = 0.27f;
            Instantiate(food, tran, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var gameObjs = GameObject.FindGameObjectsWithTag("Genetal");
        if (gameObjs.Length == 0)
        {
            var ais = JsonUtility.FromJson<List<NeuralNetwork>>(PlayerPrefs.GetString("AiList"));

            var counter = 0;
            var tran = transform.position;
            tran.z += Random.Range(-1f, 1f) * 40;
            tran.x += Random.Range(-1f, 1f) * 40;
            tran.y = 0f;
            foreach (var ai1 in ais)
            {
                foreach (var ai2 in ais)
                {
                    if (ai1 != ai2)
                    {
                        if (counter >= aiCount) { break; }

                        var childNN = ai1.CrossOver(ai2);
                        childNN.Mutation();
                        var child = Instantiate(ai, tran, transform.rotation);
                        var childAiScript = child.GetComponent<AIScript>();
                        childNN.Generation = ai1.Generation+1;
                        childAiScript.SetNN(childNN);
                        child.name = $"G-{childNN.Generation}";
                        counter++;
                    }
                }
            }
        }

        if (Time.time > nextSpawnTime)
        {
            for (int i = 0; i <Random.Range(foodCount/2, foodCount); i++)
            {
                var tran = transform.position;
                tran.z += Random.Range(-1f, 1f) * radius;
                tran.x += Random.Range(-1f, 1f) * radius;
                tran.y = 0.27f;
                Instantiate(food, tran, transform.rotation);
            }


            //increment next_spawn_time
            nextSpawnTime += Random.Range(0f, 10f);
        }
    }
}
