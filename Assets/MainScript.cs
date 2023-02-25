using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public GameObject ai;
    public GameObject food;

    public int aiCount;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0;i < aiCount; i++)
        {
            Instantiate(ai);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
