using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTime : MonoBehaviour
{
    // Start is called before the first frame update
    
    void Start()
    {
        var timeToLive = Random.Range(10f, 20f);
        Destroy(gameObject,timeToLive);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
