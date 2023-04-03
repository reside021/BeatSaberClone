using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject cubePrefab;

    public float timeSpawn = 2f;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = timeSpawn;
    }

    void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;

        if (timer < 0)
        {
            timer = timeSpawn;
            Instantiate(cubePrefab, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
